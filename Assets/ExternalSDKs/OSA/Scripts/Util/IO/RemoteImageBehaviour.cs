using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com.TheFallenGames.OSA.Util;
using System;
using Com.TheFallenGames.OSA.Util.IO.Pools;

namespace Com.TheFallenGames.OSA.Util.IO
{
    /// <summary>Utility behavior to be attached to a GameObject containing a RawImage for loading remote images using <see cref="SimpleImageDownloader"/>, optionally displaying a specific image during loading and/or on error</summary>
    [RequireComponent(typeof(RawImage))]
    public class RemoteImageBehaviour : MonoBehaviour
    {
		public delegate void LoadCompleteDelegate(bool fromCache, bool success, Vector2 textureDimension);

        [Tooltip("If not assigned, will try to find it in this game object")]
        [SerializeField] RawImage _RawImage = null;
#pragma warning disable 0649
        [SerializeField] Texture2D _LoadingTexture = null;
        [SerializeField] Texture2D _ErrorTexture = null;
#pragma warning restore 0649

        string _CurrentRequestedURL;
        bool _DestroyPending;
        Texture2D _Texture;
		IPool _Pool;


		public void InitializeWithPool(IPool pool)
		{
			_Pool = pool;
		}

        void Awake()
        {
            if (!_RawImage)
                _RawImage = GetComponent<RawImage>();
        }

        /// <summary>Starts the loading, setting the current image to <see cref="_LoadingTexture"/>, if available. If the image is already in cache, and <paramref name="loadCachedIfAvailable"/>==true, will load that instead</summary>
        public void Load(string imageURL, bool loadCachedIfAvailable = true, LoadCompleteDelegate onCompleted = null, Action onCanceled = null)
        {
			bool currentRequestedURLAlreadyLoaded = _CurrentRequestedURL == imageURL;
			_CurrentRequestedURL = imageURL;

			if (loadCachedIfAvailable)
			{
				bool foundCached = false;
				// Don't re-request if the url is the same. This is useful if there's no pool provided
				if (currentRequestedURLAlreadyLoaded)
					foundCached = _Texture != null;
				else if (_Pool != null)
				{
					Texture2D cachedInPool = _Pool.Get(imageURL) as Texture2D;
					if (cachedInPool)
					{
						_Texture = cachedInPool;
						foundCached = true;
						_CurrentRequestedURL = imageURL;
					}
				}

				if (foundCached)
				{
                    ConservAspectRatio(_Texture);
                    _RawImage.texture = _Texture;
					if (onCompleted != null)
						onCompleted(true, true, new Vector2(_Texture.width, _Texture.height));

					return;
				}
			}

            _RawImage.texture = _LoadingTexture;
            var request = new SimpleImageDownloader.Request()
            {
                url = imageURL,
                onDone = result =>
                {
                    if (!_DestroyPending && imageURL == _CurrentRequestedURL) // this will be false if a new request was done during downloading, case in which the result will be ignored
                    {
						// Commented: not reusing textures to load data into them anymore, since in most cases we'll use a pool
						//result.LoadTextureInto(_Texture);

						if (_Pool == null)
						{
							// Non-pooled textures should be destroyed
							if (_Texture)
								DisposeTexture(_Texture);

							_Texture = result.CreateTextureFromReceivedData();
						}
						else
						{
							var textureAlreadyStoredMeanwhile = _Pool.Get(imageURL);
							bool someoneStoredTheImageSooner = textureAlreadyStoredMeanwhile != null;
							if (someoneStoredTheImageSooner)
							{
								// Happens when the same URL is requested multiple times for the first time, and of course only the first 
								// downloaded image should be kept. In this case, someone else already have downloaded and cached the image, so we just discard the one we downloaded
								_Texture = textureAlreadyStoredMeanwhile as Texture2D;
							}
							else
							{
								// First time downloaded => cache
								_Texture = result.CreateTextureFromReceivedData();
								_Pool.Put(imageURL, _Texture);
							}
						}

                        ConservAspectRatio(_Texture);
                        _RawImage.texture = _Texture;

						if (onCompleted != null)
							onCompleted(false, true, new Vector2(_Texture.width, _Texture.height));
					}
					else if (onCanceled != null)
						onCanceled();
				},
                onError = () =>
                {
					if (!_DestroyPending && imageURL == _CurrentRequestedURL) // this will be false if a new request was done during downloading, case in which the result will be ignored
					{
						_RawImage.texture = _ErrorTexture;

						Vector2 dimension = Vector2.zero;
						if (_ErrorTexture != null)
							dimension = new Vector2(_ErrorTexture.width, _ErrorTexture.height);
						if (onCompleted != null)
							onCompleted(false, false, dimension);
					}
					else if (onCanceled != null)
						onCanceled();
				}
            };
            SimpleImageDownloader.Instance.Enqueue(request);
        }

        protected void ConservAspectRatio(Texture pTexture)
        {
            RectTransform imgRectTransform = _RawImage.transform.GetComponent<RectTransform>();
            if (imgRectTransform == null)
            {
                Debug.LogError("RectTransform not found in img");
                return;
            }

            RectTransform parentRectTransform = _RawImage.transform.parent.GetComponent<RectTransform>();
            if (parentRectTransform == null)
            {
                Debug.LogError("RectTransform not found in img parent");
                return;
            }

            float diffWidth = parentRectTransform.rect.width - pTexture.width;
            float diffHeight = parentRectTransform.rect.height - pTexture.height;

            float newWidth = (pTexture.width * parentRectTransform.rect.height) / pTexture.height;
            float newHeight = (pTexture.height * parentRectTransform.rect.width) / pTexture.width;

            if ((newWidth > parentRectTransform.rect.width) && (newHeight > parentRectTransform.rect.height))
            {
                if (Mathf.Abs(diffWidth) > Mathf.Abs(diffHeight))
                {
                    newWidth = parentRectTransform.rect.width;
                    newHeight = (pTexture.height * parentRectTransform.rect.width) / pTexture.width;
                }
                else
                {
                    newWidth = (pTexture.width * parentRectTransform.rect.height) / pTexture.height;
                    newHeight = parentRectTransform.rect.height;
                }
            }
            else if (newWidth > parentRectTransform.rect.width)
            {
                newWidth = parentRectTransform.rect.width;
                newHeight = (pTexture.height * parentRectTransform.rect.width) / pTexture.width;
            }
            else if (newHeight > parentRectTransform.rect.height)
            {
                newWidth = (pTexture.width * parentRectTransform.rect.height) / pTexture.height;
                newHeight = parentRectTransform.rect.height;
            }

            imgRectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }

        void OnDestroy()
        {
            _DestroyPending = true;

			// Non-pooled textures should be destroyed
			if (_Pool == null && _Texture)
			{
				DisposeTexture(_Texture);
			}
        }

		void DisposeTexture(Texture2D texture)
		{
			try
			{
				Destroy(_Texture);
			}
			catch { }
		}
    }
}
