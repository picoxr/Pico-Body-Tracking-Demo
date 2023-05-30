using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

#if !UniRxLibrary
using ObservableUnity = UniRx.Observable;
#endif

namespace UniRx.WebRequest
{
    public static class ObservableWebRequest
    {
        public static IObservable<UnityWebRequest> ToRequestObservable(this UnityWebRequest request, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<UnityWebRequest>((observer, cancellation) => Fetch(request, null, observer, progress, cancellation));
        }

        public static IObservable<string> ToObservable(this UnityWebRequest request, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<string>((observer, cancellation) => FetchText(request, null, observer, progress, cancellation));
        }

        public static IObservable<byte[]> ToBytesObservable(this UnityWebRequest request, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<byte[]>((observer, cancellation) => Fetch(request, null, observer, progress, cancellation));
        }

        public static IObservable<string> Get(string url, IDictionary<string, string> headers = null, IProgress<float> progress = null)
        {
            return
                ObservableUnity.FromCoroutine<string>(
                    (observer, cancellation) =>
                        FetchText(UnityWebRequest.Get(url), headers, observer, progress, cancellation));
        }

        public static IObservable<byte[]> GetAndGetBytes(string url, IDictionary<string, string> headers = null, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<byte[]>((observer, cancellation) => FetchBytes(UnityWebRequest.Get(url), headers, observer, progress, cancellation));
        }
        public static IObservable<UnityWebRequest> GetRequest(string url, IDictionary<string, string> headers = null, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<UnityWebRequest>((observer, cancellation) => Fetch(UnityWebRequest.Get(url), headers, observer, progress, cancellation));
        }

        public static IObservable<string> Post(string url, Dictionary<string, string> postData,
            IDictionary<string, string> headers = null, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<string>((observer, cancellation) => FetchText(UnityWebRequest.Post(url, postData), headers, observer, progress, cancellation));

        }

        public static IObservable<string> Put(string url, string putData,
            IDictionary<string, string> headers = null, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<string>((observer, cancellation) => FetchText(UnityWebRequest.Put(url, putData), headers, observer, progress, cancellation));

        }

        public static IObservable<string> PostJson(string url, string json,
    IDictionary<string, string> headers = null, IProgress<float> progress = null)
        {
            var request = new UnityWebRequest(url, "POST");
            var bodyByte = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyByte);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            return ObservableUnity.FromCoroutine<string>((observer, cancellation) => FetchText(request, headers, observer, progress, cancellation));

        }

        public static IObservable<byte[]> PostAndGetBytes(string url, Dictionary<string, string> postData, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<byte[]>((observer, cancellation) => FetchBytes(UnityWebRequest.Post(url, postData), null, observer, progress, cancellation));
        }

        public static IObservable<byte[]> PostAndGetBytes(string url, Dictionary<string, string> postData, IDictionary<string, string> headers, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<byte[]>((observer, cancellation) => FetchBytes(UnityWebRequest.Post(url, postData), headers, observer, progress, cancellation));
        }

        public static IObservable<UnityWebRequest> PostRequest(string url, Dictionary<string, string> postData, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<UnityWebRequest>((observer, cancellation) => Fetch(UnityWebRequest.Post(url, postData), null, observer, progress, cancellation));
        }

        public static IObservable<UnityWebRequest> PostRequest(string url, Dictionary<string, string> postData, IDictionary<string, string> headers, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<UnityWebRequest>((observer, cancellation) => Fetch(UnityWebRequest.Post(url, postData), headers, observer, progress, cancellation));
        }


        public static IObservable<AssetBundle> LoadFromCacheOrDownload(string url, uint version, uint crc, IProgress<float> progress = null)
        {
            return null;// ObservableUnity.FromCoroutine<AssetBundle>((observer, cancellation) => FetchAssetBundle(UnityWebRequest.GetAssetBundle(url, version, crc),null, observer, progress, cancellation));
        }


        static IEnumerator Fetch<T>(UnityWebRequest request, IDictionary<string, string> headers, IObserver<T> observer,
            IProgress<float> reportProgress, CancellationToken cancel)
        {

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }

            }

            if (reportProgress != null)
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone && !cancel.IsCancellationRequested)
                {
                    try
                    {
                        reportProgress.Report(operation.progress);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                    yield return null;
                }
            }
            else
            {
                yield return request.SendWebRequest();
            }



            if (cancel.IsCancellationRequested)
            {
                yield break;
            }

            if (reportProgress != null)
            {
                try
                {
                    reportProgress.Report(request.downloadProgress);
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    yield break;
                }
            }
        }







        static IEnumerator FetchRequest(UnityWebRequest request, IDictionary<string, string> headers, IObserver<UnityWebRequest> observer,
            IProgress<float> reportProgress, CancellationToken cancel)
        {
            using (request)
            {


                yield return Fetch(request, headers, observer, reportProgress, cancel);

                if (cancel.IsCancellationRequested)
                {
                    yield break;
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    observer.OnError(new UnityWebRequestErrorException(request));
                }
                else
                {
                    observer.OnNext(request);
                    observer.OnCompleted();
                }
            }
        }

        static IEnumerator FetchText(UnityWebRequest request, IDictionary<string, string> headers, IObserver<string> observer,
    IProgress<float> reportProgress, CancellationToken cancel)
        {
            using (request)
            {
                yield return Fetch(request, headers, observer, reportProgress, cancel);

                if (cancel.IsCancellationRequested)
                {
                    yield break;
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    observer.OnError(new UnityWebRequestErrorException(request));
                }
                else
                {
                    var text = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                    observer.OnNext(text);
                    observer.OnCompleted();
                }
            }
        }

        static IEnumerator FetchAssetBundle(UnityWebRequest request, IDictionary<string, string> headers, IObserver<AssetBundle> observer,
    IProgress<float> reportProgress, CancellationToken cancel)
        {
            using (request)
            {
                yield return Fetch(request, headers, observer, reportProgress, cancel);

                if (cancel.IsCancellationRequested)
                {
                    yield break;
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    observer.OnError(new UnityWebRequestErrorException(request));
                }
                else
                {
                    var handler = request.downloadHandler as DownloadHandlerAssetBundle;
                    var assetBundle = (handler != null) ? handler.assetBundle : null;

                    observer.OnNext(assetBundle);
                    observer.OnCompleted();
                }
            }
        }

        static IEnumerator FetchBytes(UnityWebRequest request, IDictionary<string, string> headers, IObserver<byte[]> observer,
    IProgress<float> reportProgress, CancellationToken cancel)
        {
            using (request)
            {
                yield return Fetch(request, headers, observer, reportProgress, cancel);

                if (cancel.IsCancellationRequested)
                {
                    yield break;
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    observer.OnError(new UnityWebRequestErrorException(request));
                }
                else
                {
                    observer.OnNext(request.downloadHandler.data);
                    observer.OnCompleted();
                }
            }
        }




    }

    public class UnityWebRequestErrorException : Exception
    {
        public string RawErrorMessage { get; private set; }
        public bool HasResponse { get; private set; }
        public string Text { get; private set; }
        public System.Net.HttpStatusCode StatusCode { get; private set; }
        public System.Collections.Generic.Dictionary<string, string> ResponseHeaders { get; private set; }
        public UnityWebRequest Request { get; private set; }

        // cache the text because if www was disposed, can't access it.
        public UnityWebRequestErrorException(UnityWebRequest request)
        {
            this.Request = request;
            this.RawErrorMessage = request.error;
            this.ResponseHeaders = request.GetResponseHeaders();
            this.HasResponse = false;

            StatusCode = (System.Net.HttpStatusCode)request.responseCode;


            if (request.downloadHandler != null)
            {
                Text = request.downloadHandler.text;
            }

            if (request.responseCode != 0)
            {
                this.HasResponse = true;
            }
        }

        public override string ToString()
        {
            var text = this.Text;
            if (string.IsNullOrEmpty(text))
            {
                return RawErrorMessage;
            }
            else
            {
                return RawErrorMessage + " " + text;
            }
        }
    }
}