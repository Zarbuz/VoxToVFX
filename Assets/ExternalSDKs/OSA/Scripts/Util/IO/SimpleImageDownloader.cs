// WWW class usage
#pragma warning disable 0618

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace Com.TheFallenGames.OSA.Util.IO
{
    /// <summary>
    /// <para>A utility singleton class for downloading images using a LIFO queue for the requests. <see cref="MaxConcurrentRequests"/> can be used to limit the number of concurrent requests. </para> 
    /// <para>Default is <see cref="DEFAULT_MAX_CONCURRENT_REQUESTS"/>. Each request is executed immediately if there's room for it. When the queue is full, the downloder starts checking each second if a slot is freed, after which re-enters the loop.</para> 
    /// </summary>
    public class SimpleImageDownloader : MonoBehaviour
    {
        public static SimpleImageDownloader Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GameObject(typeof(SimpleImageDownloader).Name).AddComponent<SimpleImageDownloader>();

                return _Instance;
            }
        }

        static SimpleImageDownloader _Instance;


        public int MaxConcurrentRequests { get; set; }

        const int DEFAULT_MAX_CONCURRENT_REQUESTS = 20;

        List<Request> _QueuedRequests = new List<Request>();
        List<Request> _ExecutingRequests = new List<Request>();
        WaitForSeconds _Wait1Sec = new WaitForSeconds(1f);


        IEnumerator Start()
        {
            if (MaxConcurrentRequests == 0)
                MaxConcurrentRequests = DEFAULT_MAX_CONCURRENT_REQUESTS;

            while (true)
            {
                while (_ExecutingRequests.Count >= MaxConcurrentRequests)
                {
                    yield return _Wait1Sec;
                }

                int lastIndex = _QueuedRequests.Count - 1;
                if (lastIndex >= 0)
                {
                    var lastRequest = _QueuedRequests[lastIndex];
                    _QueuedRequests.RemoveAt(lastIndex);

                    StartCoroutine(DownloadCoroutine(lastRequest));
                }

                yield return null;
            }
        }

		void OnDestroy()
		{
			_Instance = null;
		}

		public void Enqueue(Request request)
        { _QueuedRequests.Add(request); }

        IEnumerator DownloadCoroutine(Request request)
        {
            _ExecutingRequests.Add(request);
            var www = new WWW(request.url);

            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                if (request.onDone != null)
                {
                    var result = new Result(www);
                    request.onDone(result);
                }
            }
            else
            {
                if (request.onError != null)
                    request.onError();
            }
            www.Dispose();
            _ExecutingRequests.Remove(request);
        }

        public class Request
        {
            public string url;
            public Action<Result> onDone;
            public Action onError;
        }

        public class Result
        {
            WWW _UsedWWW;


			public Result(WWW www)
            { _UsedWWW = www; }


            public Texture2D CreateTextureFromReceivedData()
            { return _UsedWWW.texture; }

            public void LoadTextureInto(Texture2D existingTexture)
            { _UsedWWW.LoadImageIntoTexture(existingTexture); }
        }
    }
}
#pragma warning restore 0618