// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using TMPro;
using UnityEngine;

namespace AutoMachineRebuilt.UI.TMP
{
    internal class TMPImportFix : KMonoBehaviour
    {
        [SerializeField]
        public TextOverflowModes textOverflow;

        [SerializeField]
        public TextAlignmentOptions alignment;

        [SerializeField]
        public float fontSizeMin;

        [SerializeField]
        public float fontSizeMax;

        [SerializeField]
        public bool autoResize;

#pragma warning disable CS0649
        [MyCmpReq]
        private LocText text;
#pragma warning restore CS0649

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (text != null)
            {
                text.alignment = alignment;
                text.overflowMode = textOverflow;
                text.fontSizeMax = fontSizeMax;
                text.fontSizeMin = fontSizeMin;
                text.enableAutoSizing = autoResize;
            }
            Destroy(this);
        }
    }
}
