/*
* This file is a copy of Unity's [ConditionalCompilationUtility](https://github.com/Unity-Technologies/ConditionalCompilationUtility/tree/f364090bbda3728e1662074c969c2b7c3c34199b)
* with the following modifications:
* - changed the namespace
* - changed k_EnableCCU from UNITY_CCU to VUPLEX_CCU
* - pasted the license below
*
* Unity Companion License 1.0 ("License")
* Copyright (C) 2017-2018 Unity Technologies ApS ("Unity")
*
* Unity hereby grants to you a worldwide, non-exclusive, no-charge, and royalty-free copyright license to reproduce, prepare derivative works of, publicly display, publicly perform, sublicense, and distribute the software that is made available with this License ("Software"), subject to the following terms and conditions:
*
* 1. Unity Companion Use Only. Exercise of the license granted herein is limited to exercise for the creation, use, and/or distribution of applications, software, or other content pursuant to a valid Unity development engine software license ("Engine License"). That means while use of the Software is not limited to use in the software licensed under the Engine License, the Software may not be used for any purpose other than the creation, use, and/or distribution of Engine License-dependent applications, software, or other content. No other exercise of the license granted herein is permitted.
*
* 2. No Modification of Engine License. Neither this License nor any exercise of the license granted herein modifies the Engine License in any way.
*
* 3. Ownership & Grant Back to You.
*
* 3.1 You own your content. In this License, "derivative works" means derivatives of the Software itself--works derived only from the Software by you under this License (for example, modifying the code of the Software itself to improve its efficacy); "derivative works" of the Software do not include, for example, games, apps, or content that you create using the Software. You keep all right, title, and interest to your own content.
*
* 3.2 Unity owns its content. While you keep all right, title, and interest to your own content per the above, as between Unity and you, Unity will own all right, title, and interest to all intellectual property rights (including patent, trademark, and copyright) in the Software and derivative works of the Software, and you hereby assign and agree to assign all such rights in those derivative works to Unity.
*
* 3.3 You have a license to those derivative works. Subject to this License, Unity grants to you the same worldwide, non-exclusive, no-charge, and royalty-free copyright license to derivative works of the Software you create as is granted to you for the Software under this License.
*
* 4. Trademarks. You are not granted any right or license under this License to use any trademarks, service marks, trade names, products names, or branding of Unity or its affiliates ("Trademarks"). Descriptive uses of Trademarks are permitted; see, for example, Unity's Branding Usage Guidelines at https://unity3d.com/public-relations/brand.
*
* 5. Notices & Third-Party Rights. This License, including the copyright notice above, must be provided in all substantial portions of the Software and derivative works thereof (or, if that is impracticable, in any other location where such notices are customarily placed). Further, if the Software is accompanied by a Unity "third-party notices" or similar file, you acknowledge and agree that software identified in that file is governed by those separate license terms.
*
* 6. DISCLAIMER, LIMITATION OF LIABILITY. THE SOFTWARE AND ANY DERIVATIVE WORKS THEREOF IS PROVIDED ON AN "AS IS" BASIS, AND IS PROVIDED WITHOUT WARRANTY OF ANY KIND, WHETHER EXPRESS OR IMPLIED, INCLUDING ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND/OR NONINFRINGEMENT. IN NO EVENT SHALL ANY COPYRIGHT HOLDER OR AUTHOR BE LIABLE FOR ANY CLAIM, DAMAGES (WHETHER DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL, INCLUDING PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, LOSS OF USE, DATA, OR PROFITS, AND BUSINESS INTERRUPTION), OR OTHER LIABILITY WHATSOEVER, WHETHER IN AN ACTION OF CONTRACT, TORT, OR OTHERWISE, ARISING FROM OR OUT OF, OR IN CONNECTION WITH, THE SOFTWARE OR ANY DERIVATIVE WORKS THEREOF OR THE USE OF OR OTHER DEALINGS IN SAME, EVEN WHERE ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
* 7. USE IS ACCEPTANCE and License Versions. Your receipt and use of the Software constitutes your acceptance of this License and its terms and conditions. Software released by Unity under this License may be modified or updated and the License with it; upon any such modification or update, you will comply with the terms of the updated License for any use of any of the Software under the updated License.
*
* 8. Use in Compliance with Law and Termination. Your exercise of the license granted herein will at all times be in compliance with applicable law and will not infringe any proprietary rights (including intellectual property rights); this License will terminate immediately on any breach by you of this License.
*
* 9. Severability. If any provision of this License is held to be unenforceable or invalid, that provision will be enforced to the maximum extent possible and the other provisions will remain in full force and effect.
*
* 10. Governing Law and Venue. This License is governed by and construed in accordance with the laws of Denmark, except for its conflict of laws rules; the United Nations Convention on Contracts for the International Sale of Goods will not apply. If you reside (or your principal place of business is) within the United States, you and Unity agree to submit to the personal and exclusive jurisdiction of and venue in the state and federal courts located in San Francisco County, California concerning any dispute arising out of this License ("Dispute"). If you reside (or your principal place of business is) outside the United States, you and Unity agree to submit to the personal and exclusive jurisdiction of and venue in the courts located in Copenhagen, Denmark concerning any Dispute.
*/
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;
using System.Threading;

namespace Vuplex.WebView.ConditionalCompilation
{
    /// <summary>
    /// The Conditional Compilation Utility (CCU) will add defines to the build settings once dependendent classes have been detected.
    /// A goal of the CCU was to not require the CCU itself for other libraries to specify optional dependencies. So, it relies on the
    /// specification of at least one custom attribute in a project that makes use of it. Here is an example:
    ///
    /// [Conditional(UNITY_CCU)]                                    // | This is necessary for CCU to pick up the right attributes
    /// public class OptionalDependencyAttribute : Attribute        // | Must derive from System.Attribute
    /// {
    ///     public string dependentClass;                           // | Required field specifying the fully qualified dependent class
    ///     public string define;                                   // | Required field specifying the define to add
    /// }
    ///
    /// Then, simply specify the assembly attribute(s) you created in any of your C# files:
    /// [assembly: OptionalDependency("UnityEngine.InputNew.InputSystem", "USE_NEW_INPUT")]
    /// [assembly: OptionalDependency("Valve.VR.IVRSystem", "ENABLE_STEAMVR_INPUT")]
    ///
    /// namespace Foo
    /// {
    /// ...
    /// }
    /// </summary>
    [InitializeOnLoad]
    static class ConditionalCompilationUtility
    {
        const string k_PreviousUnsuccessfulDefines = "ConditionalCompilationUtility.PreviousUnsuccessfulDefines";
        const string k_EnableCCU = "VUPLEX_CCU";

        public static bool enabled
        {
            get
            {
                var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Contains(k_EnableCCU);
            }
        }

        public static string[] defines { private set; get; }

        static ConditionalCompilationUtility()
        {
#if UNITY_2017_3_OR_NEWER
            var errorsFound = false;
            CompilationPipeline.assemblyCompilationFinished += (outputPath, compilerMessages) =>
            {
                var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error && m.message.Contains("CS0246"));
                if (errorCount > 0 && !errorsFound)
                {
                    var previousDefines = EditorPrefs.GetString(k_PreviousUnsuccessfulDefines);
                    var currentDefines = string.Join(";", defines);
                    if (currentDefines != previousDefines)
                    {
                        // Store the last set of unsuccessful defines to avoid ping-ponging
                        EditorPrefs.SetString(k_PreviousUnsuccessfulDefines, currentDefines);

                        // Since there were errors in compilation, try removing any dependency defines
                        UpdateDependencies(true);
                    }
                    errorsFound = true;
                }
            };

            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                if (!errorsFound)
                    UpdateDependencies();
            };
#else
            UpdateDependencies();
#endif
        }

        static void UpdateDependencies(bool reset = false)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (buildTargetGroup == BuildTargetGroup.Unknown)
            {
                var propertyInfo = typeof(EditorUserBuildSettings).GetProperty("activeBuildTargetGroup", BindingFlags.Static | BindingFlags.NonPublic);
                if (propertyInfo != null)
                    buildTargetGroup = (BuildTargetGroup)propertyInfo.GetValue(null, null);
            }

            var previousProjectDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var projectDefines = previousProjectDefines.Split(';').ToList();
            if (!projectDefines.Contains(k_EnableCCU, StringComparer.OrdinalIgnoreCase))
            {
                EditorApplication.LockReloadAssemblies();

                projectDefines.Add(k_EnableCCU);

                // This will trigger another re-compile, which needs to happen, so all the custom attributes will be visible
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", projectDefines.ToArray()));

                // Let other systems execute before reloading assemblies
                Thread.Sleep(1000);
                EditorApplication.UnlockReloadAssemblies();

                return;
            }

            var ccuDefines = new List<string> { k_EnableCCU };

            var conditionalAttributeType = typeof(ConditionalAttribute);

            const string kDependentClass = "dependentClass";
            const string kDefine = "define";

            var attributeTypes = GetAssignableTypes(typeof(Attribute), type =>
            {
                var conditionals = (ConditionalAttribute[])type.GetCustomAttributes(conditionalAttributeType, true);

                foreach (var conditional in conditionals)
                {
                    if (string.Equals(conditional.ConditionString, k_EnableCCU, StringComparison.OrdinalIgnoreCase))
                    {
                        var dependentClassField = type.GetField(kDependentClass);
                        if (dependentClassField == null)
                        {
                            Debug.LogErrorFormat("[CCU] Attribute type {0} missing field: {1}", type.Name, kDependentClass);
                            return false;
                        }

                        var defineField = type.GetField(kDefine);
                        if (defineField == null)
                        {
                            Debug.LogErrorFormat("[CCU] Attribute type {0} missing field: {1}", type.Name, kDefine);
                            return false;
                        }

                        return true;
                    }
                }

                return false;
            });

            var dependencies = new Dictionary<string, string>();
            ForEachAssembly(assembly =>
            {
                var typeAttributes = assembly.GetCustomAttributes(false).Cast<Attribute>();
                foreach (var typeAttribute in typeAttributes)
                {
                    if (attributeTypes.Contains(typeAttribute.GetType()))
                    {
                        var t = typeAttribute.GetType();

                        // These fields were already validated in a previous step
                        var dependentClass = t.GetField(kDependentClass).GetValue(typeAttribute) as string;
                        var define = t.GetField(kDefine).GetValue(typeAttribute) as string;

                        if (!string.IsNullOrEmpty(dependentClass) && !string.IsNullOrEmpty(define) && !dependencies.ContainsKey(dependentClass))
                            dependencies.Add(dependentClass, define);
                    }
                }
            });


            ForEachAssembly(assembly =>
            {
                foreach (var dependency in dependencies)
                {
                    var typeName = dependency.Key;
                    var define = dependency.Value;

                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        if (!projectDefines.Contains(define, StringComparer.OrdinalIgnoreCase))
                            projectDefines.Add(define);

                        ccuDefines.Add(define);
                    }
                }
            });

            if (reset)
            {
                foreach (var define in dependencies.Values)
                {
                    projectDefines.Remove(define);
                }

                ccuDefines.Clear();
                ccuDefines.Add(k_EnableCCU);
            }

            ConditionalCompilationUtility.defines = ccuDefines.ToArray();

            var newDefines = string.Join(";", projectDefines.ToArray());
            if (previousProjectDefines != newDefines)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
        }

        static void ForEachAssembly(Action<Assembly> callback)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    callback(assembly);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip any assemblies that don't load properly
                    continue;
                }
            }
        }

        static void ForEachType(Action<Type> callback)
        {
            ForEachAssembly(assembly =>
            {
                var types = assembly.GetTypes();
                foreach (var t in types)
                    callback(t);
            });
        }

        static IEnumerable<Type> GetAssignableTypes(Type type, Func<Type, bool> predicate = null)
        {
            var list = new List<Type>();
            ForEachType(t =>
            {
                if (type.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && (predicate == null || predicate(t)))
                    list.Add(t);
            });

            return list;
        }
    }
}
#endif
