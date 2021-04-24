### D2SLib

Simple C# library for reading and writing Diablo 2 saves. Supports version 1.10 through Diablo II: Resurrected (1.15). Supports reading both d2s (player saves) and d2i (shared stash) files.

Example:
```
using D2SLib;
using D2SLib.Model.Save;

....

D2S character = Core.ReadD2S(File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s"));

//outputs: DannyIsGreat
Console.WriteLine(character.Name);

 //set all skills to have 20 points
character.ClassSkills.Skills.ForEach(skill => skill.Points = 20);

//add lvl 31 conviction arua to the first statlist on the first item in your chars inventory
character.PlayerItemList.Items[0].StatLists[0].Stats.Add(new ItemStat { Stat = "item_aura", Param = 123, Value = 31 });

//write save
File.WriteAllBytes(Environment.ExpandEnvironmentVariables($"%userprofile%/Saved Games/Diablo II Resurrected Tech Alpha/{character.Name}.d2s"), Core.WriteD2S(character));

```

TODO: let others seed w/ their own mod txt files. currently this can be done if you build it yourself.

##### Useful Links:
* https://github.com/d07RiV/d07riv.github.io/blob/master/d2r.html (credits to d07riv for reversing the item code on D2R)
* https://github.com/nokka/d2s
* https://github.com/krisives/d2s-format
* http://paul.siramy.free.fr/d2ref/eng/
* http://user.xmission.com/~trevin/DiabloIIv1.09_File_Format.shtml
* https://github.com/nickshanks/Alkor
* https://github.com/HarpyWar/d2s-character-editor
