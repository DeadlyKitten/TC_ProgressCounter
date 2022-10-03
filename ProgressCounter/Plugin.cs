using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProgressCounter
{
    [HarmonyPatch]
    [BepInPlugin("com.steven.trombone.progresscounter", "Progress Counter", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool usePercent = false;

        void Awake()
        {
            var harmony = new Harmony("com.steven.trombone.progresscounter");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(GameController), "Start")]
        static void Postfix(GameController __instance, float ___levelendtime)
        {
            if (__instance.freeplay) return;

            var combo = GameObject.Find("maxcombo");
            var counter = Instantiate(combo, combo.transform.position, combo.transform.rotation).AddComponent<Counter>();
            counter.gameObject.name = "Progress Counter";
            counter.transform.parent = combo.transform.parent;
            counter.Init(__instance.musictrack, ___levelendtime);
        }

        class Counter : MonoBehaviour
        {
            private Text _foregroundText;
            private Text _shadowText;

            private AudioSource _musicTrack;
            private float _levelEndTime;

            private string _totalSongTimeString;
            private string _timeStringTemplate;

            public void Init(AudioSource musicTrack, float levelEndTime)
            {
                _musicTrack = musicTrack;
                _levelEndTime = levelEndTime;

                var levelEndTimespan = TimeSpan.FromSeconds(_levelEndTime);
                _timeStringTemplate = (levelEndTimespan.TotalMinutes > 9) ? @"mm\:ss" : @"m\:ss";
                _totalSongTimeString = levelEndTimespan.ToString(_timeStringTemplate);

                transform.localScale = Vector3.one;
            }

            private void Start()
            {
                _foregroundText = transform.Find("maxcombo_shadow/maxcombo_text").GetComponent<Text>();
                _shadowText = transform.Find("maxcombo_shadow").GetComponent<Text>();

                var rect = GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 15);
            }

            private void Update()
            {
                var currentTimeTimespan = TimeSpan.FromSeconds(_musicTrack.time);
                var currentTimeString = currentTimeTimespan.ToString(_timeStringTemplate);

                string counterText;
                if (usePercent)
                    counterText = string.Format("Progress: {0:P0}", _musicTrack.time / _levelEndTime);
                else
                    counterText = $"{currentTimeString} / {_totalSongTimeString}";

                _foregroundText.text = counterText;
                _shadowText.text = counterText;
            }
        }
    }
}
