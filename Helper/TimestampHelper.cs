using System;
using System.Collections.Generic;
using System.Linq;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Parser.Statics;

namespace MapsetChecksCatch.Helper
{
    public class TimestampHelper
    {
        public static string Get(params CatchHitObject[] catchHitObject)
        {
            var timestampObjects = new List<CatchHitObject>();
            foreach (var hitObject in catchHitObject)
            {
                switch (hitObject.NoteType)
                {
                    case NoteType.CIRCLE:
                    case NoteType.HEAD:
                        timestampObjects.Add(hitObject);
                        break;
                    case NoteType.DROPLET:
                    case NoteType.REPEAT:
                    case NoteType.TAIL:
                        if (!timestampObjects.Contains(hitObject.SliderHead))
                        {
                            timestampObjects.Add(hitObject.SliderHead);
                        }
                        break;
                }
            }

            return Timestamp.Get(timestampObjects.Select(x => x.Original).ToArray());
        }
    }
}