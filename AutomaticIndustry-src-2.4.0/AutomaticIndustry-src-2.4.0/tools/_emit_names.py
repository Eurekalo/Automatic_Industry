import json
d=json.load(open('/tmp/gen/names.json'))
def esc(s): return s.replace('\\','\\\\').replace('"','\\"')
def block(field, lang, doc):
    lines=[f'        /// <summary>{doc}</summary>',
           f'        internal static readonly Dictionary<string, string> {field} = new Dictionary<string, string>',
           '        {']
    items=[f'            {{ "{k}", "{esc(v[lang])}" }}' for k,v in d.items() if v[lang]]
    lines.append(',\n'.join(items))
    lines.append('        };')
    return '\n'.join(lines)
head='''// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).
//
// GENERATED FILE - do not edit by hand.
//
// Every string below is copied verbatim from the localization files shipped
// with Oxygen Not Included (OxygenNotIncluded_Data/StreamingAssets/strings):
//
//   English             strings_template.pot            (source strings)
//   Simplified Chinese  strings_preinstalled_zh_klei.po (official Klei)
//   Korean              strings_preinstalled_ko_klei.po (official Klei)
//   Traditional Chinese converted from the official Simplified Chinese text
//                       with OpenCC (s2twp), because Klei ships no zh-Hant file
//
// The key of every row is the mod option key; the value is the vanilla
// building name with all rich text markup removed. Japanese is intentionally
// absent: Oxygen Not Included ships no official Japanese localization, so the
// Japanese fallback chain is resolved at runtime (see BuildingDescriptions).
//
// Regenerate with: tools/generate_vanilla_names.py

using System.Collections.Generic;
using AutoMachineRebuilt.Config;

namespace AutoMachineRebuilt.Localization
{
    /// <summary>
    /// Vanilla building names in every language the game itself ships, keyed
    /// by the mod option key. Used as the single source of truth for every
    /// building label shown by the mod, so a translated label can never drift
    /// away from the wording the player sees in game.
    /// </summary>
    internal static class VanillaBuildingNames
    {
'''
body='\n\n'.join([
 block('English','en','Vanilla English building names (bilingual base text).'),
 block('ChineseSimplified','zhcn','Official Klei Simplified Chinese building names.'),
 block('ChineseTraditional','zhtw','Traditional Chinese, converted from the official Simplified Chinese text.'),
 block('Korean','ko','Official Klei Korean building names.'),
])
tail='''

        /// <summary>
        /// Returns the vanilla name table of a language, or <c>null</c> when
        /// the game ships no localization for it and the runtime fallback
        /// chain has to be used instead.
        /// </summary>
        /// <param name="language">Resolved options language.</param>
        internal static Dictionary<string, string> For(UiLanguage language)
        {
            switch (language)
            {
                case UiLanguage.ChineseSimplified:
                    return ChineseSimplified;
                case UiLanguage.ChineseTraditional:
                    return ChineseTraditional;
                case UiLanguage.Korean:
                    return Korean;
                case UiLanguage.English:
                    return English;
                default:
                    return null;
            }
        }
    }
}
'''
open('/mnt/documents/AutoMachineRebuilt/src/Localization/VanillaBuildingNames.generated.cs','w',encoding='utf-8').write(head+body+tail)
print('ok')
