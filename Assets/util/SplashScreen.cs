using UnityEngine;
using System.Collections;
namespace sw.util
{
    public class SplashScreen : MonoBehaviour
    {
        public string LevelToLoad = "main";
        public Texture2D SplashLogo;
        public float FadeSpeed = 0.8F;
        public float WaitTime = 2F;
        bool bLoad = false;
        #region 渐入渐出的类型
        public enum SplashType
        {
            LoadLevelThenFadeOut,
            FadeOutThenLoadLevel
        }
        public SplashType Type = SplashType.LoadLevelThenFadeOut;
        #endregion

        #region 渐入渐出的状态
        public enum FadeStatus
        {
            FadeIn,
            FadeWait,
            FadeOut
        }
        private FadeStatus mStatus = FadeStatus.FadeIn;
        #endregion

        public bool WaitForInput = true;
        private float mAlpha = 0.0F;
        private Rect mSplashLogoPos;
        private float mFadeInFinishedTime;
        AsyncOperation async;
        void Start()
        {
            mSplashLogoPos.x = (Screen.width * 0.5F - SplashLogo.width * 0.5F);
            mSplashLogoPos.y = (Screen.height * 0.5F - SplashLogo.height * 0.5F);
            mSplashLogoPos.width = SplashLogo.width;
            mSplashLogoPos.height = SplashLogo.height;
            if ((Application.levelCount <= 1) || (LevelToLoad == ""))
                return;
            StartCoroutine("loadMainSence");       
        }

        void Update()
        {
            switch (mStatus)
            {
                case FadeStatus.FadeIn:
                    mAlpha += FadeSpeed * Time.deltaTime;
                    break;
                case FadeStatus.FadeOut:
                    mAlpha -= FadeSpeed * Time.deltaTime;
                    break;
                case FadeStatus.FadeWait:
                    if ((!WaitForInput && Time.time > mFadeInFinishedTime + WaitTime) || (WaitForInput && Input.anyKey))
                    {
                        mStatus = FadeStatus.FadeOut;
                    }
                    break;
            }
           
        }

        void OnGUI()
        {
            if (SplashLogo != null)
            {
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, Mathf.Clamp(mAlpha, 0, 1));
                GUI.DrawTexture(mSplashLogoPos, SplashLogo, ScaleMode.ScaleAndCrop);

                if (mAlpha > 1.0F)
                {
                    mStatus = FadeStatus.FadeWait;
                    mFadeInFinishedTime = Time.time;
                    mAlpha = 1.0F;
                }

                if (mAlpha < 0.0F)
                {
                    if (Type == SplashType.FadeOutThenLoadLevel)
                    {
                        mStatus = FadeStatus.FadeOut;
                    }
                    else
                    {
                        Destroy(this);
                    }
                }
            }

            if (mStatus == FadeStatus.FadeWait)
            {
                mStatus = FadeStatus.FadeOut;
            }
        }

        public IEnumerator loadMainSence()
        {
            yield return new WaitForEndOfFrame();
            async = Application.LoadLevelAsync("main");
            yield return async;
        }
    }
}
