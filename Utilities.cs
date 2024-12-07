using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS_CustomOST
{
    internal static class Utilities
    {
        public static GameObject GetChildWithName(this Transform tr, string name)
        {
            for (int i = 0; i < tr.childCount; i++)
            {
                if (tr.GetChild(i).name == name)
                {
                    return tr.GetChild(i).gameObject;
                }
            }

            return null;
        }

        public static GameObject GetChildWithName(this GameObject obj, string name)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                if (obj.transform.GetChild(i).name == name)
                {
                    return obj.transform.GetChild(i).gameObject;
                }
            }

            return null;
        }

        public static Transform[] GetChilds(this Transform tr)
        {
            List<Transform> transforms = new List<Transform>();

            for (int i = 0; i < tr.childCount; i++)
            {
                transforms.Add(tr.GetChild(i));
            }

            return transforms.ToArray();
        }

        public static GameObject[] GetChilds(this GameObject obj)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                gameObjects.Add(obj.transform.GetChild(i).gameObject);
            }

            return gameObjects.ToArray();
        }

        public static void PlayIgnoringTimeScale(this TweenAlpha tween, bool reversed)
        {
            tween.ignoreTimeScale = true;
            if (reversed) tween.PlayReverse(); else tween.PlayForward();
        }
        public static void PlayIgnoringTimeScale(this TweenScale tween, bool reversed)
        {
            tween.ignoreTimeScale = true;
            if (reversed) tween.PlayReverse(); else tween.PlayForward();
        }

        public static void Invoke(this Il2CppSystem.Collections.Generic.List<EventDelegate> onClick)
        {
            foreach (var @event in onClick)
            {
                @event.Execute();
            }
        }

        public static void SetIsEnabled(this UIPopupList list, bool isEnabled)
        {
            if (isEnabled)
            {
                list.transform.GetComponent<UIWidget>().enabled = true;
                list.transform.GetChildWithName("Background").GetComponent<UISprite>().color = new Color(0f, 0.6792f, 0.6451f, 1f);
            }
            else
            {
                list.transform.GetComponent<UIWidget>().enabled = false;
                list.transform.GetChildWithName("Background").GetComponent<UISprite>().color = new Color(0.4191f, 0.4191f, 0.4191f, 0.897f);
            }
        }
    }
}
