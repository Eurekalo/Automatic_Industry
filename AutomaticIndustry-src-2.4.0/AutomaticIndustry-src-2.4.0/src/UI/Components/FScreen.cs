// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using UnityEngine;

namespace AutoMachineRebuilt.UI.Components
{
    public class FScreen : KScreen
    {
        public const float SCREEN_SORT_KEY = 300f;

        private bool shown;
        public bool pause = true;
        public bool lockCam = true;

        protected override void OnPrefabInit()
        {
            activateOnSpawn = true;
            gameObject.SetActive(true);
        }

        public virtual void ShowDialog()
        {
            if (transform.parent != null && transform.parent.GetComponent<Canvas>() == null && transform.parent.parent != null)
            {
                transform.SetParent(transform.parent.parent);
            }
            transform.SetAsLastSibling();
        }

        public virtual void OnClickCancel()
        {
            Reset();
            Deactivate();
        }

        public virtual void Reset()
        {
        }

        public virtual void OnClickApply()
        {
        }

        protected override void OnCmpEnable()
        {
            base.OnCmpEnable();
            if (lockCam && CameraController.Instance != null)
            {
                CameraController.Instance.DisableUserCameraControl = true;
            }
        }

        protected override void OnCmpDisable()
        {
            base.OnCmpDisable();
            if (lockCam && CameraController.Instance != null)
            {
                CameraController.Instance.DisableUserCameraControl = false;
            }
        }

        public override bool IsModal()
        {
            return true;
        }

        public override float GetSortKey()
        {
            return SCREEN_SORT_KEY;
        }

        protected override void OnActivate()
        {
            OnShow(true);
        }

        protected override void OnDeactivate()
        {
            OnShow(false);
        }

        protected override void OnShow(bool show)
        {
            base.OnShow(show);
            if (pause && SpeedControlScreen.Instance != null)
            {
                if (show && !shown)
                {
                    SpeedControlScreen.Instance.Pause(false, false);
                }
                else if (!show && shown)
                {
                    SpeedControlScreen.Instance.Unpause(false);
                }
                shown = show;
            }
        }

        public override void OnKeyUp(KButtonEvent e)
        {
            if (!e.Consumed)
            {
                KScrollRect componentInChildren = GetComponentInChildren<KScrollRect>();
                if (componentInChildren != null)
                {
                    componentInChildren.OnKeyUp(e);
                }
            }
            e.Consumed = true;
        }
    }
}
