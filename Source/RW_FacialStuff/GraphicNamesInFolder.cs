using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class GraphicDatabaseUtilityFS
    {
        public static IEnumerable<string> GraphicNamesInFolder(string folderPath)
        {
            IEnumerable<Texture2D> allInFolder = ContentFinder<Texture2D>.GetAllInFolder(folderPath);
            List<string> list = new List<string>();
            using (IEnumerator<Texture2D> enumerator = allInFolder.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    List<string> list2 = enumerator.Current?.name?.Split("_".ToCharArray()).ToList();
                    if (list2 == null)
                    {
                        break;
                    }

                    if (list2.Count > 4)
                    {
                        Log.Error("Cannot load assets with >3 pieces.");
                    }
                    else if (list2.Count == 1 && !list.Contains(list2[0]))
                    {
                        list.Add(list2[0]);
                    }
                    else
                    {
                        list2.Remove(list2.Last());
                        string item = string.Join("_", list2.ToArray());
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                }
            }

            return list;
        }
    }
}
