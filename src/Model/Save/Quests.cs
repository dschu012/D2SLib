using D2SLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace D2SLib.Model.Save
{
    public class QuestsSection
    {
        //0x014b [unk = 0x1, 0x0, 0x0, 0x0]
        public UInt32? Magic { get; set; }
        //0x014f [quests header identifier = 0x57, 0x6f, 0x6f, 0x21 "Woo!"]
        public UInt32? Header { get; set; }
        //0x0153 [version = 0x6, 0x0, 0x0, 0x0]
        public UInt32? Version { get; set; }
        //0x0153 [quests header length = 0x2a, 0x1]
        public UInt16? Length { get; set; }

        public QuestsDifficulty Normal { get; set; }
        public QuestsDifficulty Nightmare { get; set; }
        public QuestsDifficulty Hell { get; set; }

        public static QuestsSection Read(byte[] bytes)
        {
            QuestsSection questSection = new QuestsSection();
            using (BitReader reader = new BitReader(bytes))
            {
                questSection.Magic = reader.ReadUInt32();
                questSection.Header = reader.ReadUInt32();
                questSection.Version = reader.ReadUInt32();
                questSection.Length = reader.ReadUInt16();
                var skippedProperties = new string[]{ "Magic", "Header", "Version", "Length" };
                foreach (var property in typeof(QuestsSection).GetProperties())
                {
                    if (skippedProperties.Contains(property.Name)) continue;
                    property.SetValue(questSection, QuestsDifficulty.Read(reader.ReadBytes(96)));
                }
                return questSection;
            }
        }

        public static byte[] Write(QuestsSection questSection)
        {
            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt32(questSection.Magic ?? 0x1);
                writer.WriteUInt32(questSection.Header ?? 0x216F6F57);
                writer.WriteUInt32(questSection.Version ?? 0x6);
                writer.WriteUInt16(questSection.Length ?? (UInt16)0x12A);
                var skippedProperties = new string[] { "Magic", "Header", "Version", "Length" };
                foreach (var property in typeof(QuestsSection).GetProperties())
                {
                    if (skippedProperties.Contains(property.Name)) continue;
                    QuestsDifficulty questsDifficulty = (QuestsDifficulty)property.GetValue(questSection);
                    writer.WriteBytes(QuestsDifficulty.Write(questsDifficulty));
                }
                return writer.ToArray();
            }
        }
    }

    public class QuestsDifficulty
    {
        public ActIQuests ActI { get; set; }
        public ActIIQuests ActII { get; set; }
        public ActIIIQuests ActIII { get; set; }
        public ActIVQuests ActIV { get; set; }
        public ActVQuests ActV { get; set; }

        public static QuestsDifficulty Read(byte[] bytes)
        {
            QuestsDifficulty questsDifficulty = new QuestsDifficulty();
            using (BitReader reader = new BitReader(bytes))
            {
                Type questsDifficultyType = typeof(QuestsDifficulty);
                foreach (var questsDifficultyProperty in questsDifficultyType.GetProperties())
                {
                    Type type = questsDifficultyProperty.PropertyType;
                    var quests = Activator.CreateInstance(type);
                    foreach (var property in type.GetProperties())
                    {
                        Quest quest = new Quest();
                        property.SetValue(quests, Quest.Read(reader.ReadBytes(2)));
                    }
                    questsDifficultyProperty.SetValue(questsDifficulty, quests);
                }
                return questsDifficulty;
            }
        }

        public static byte[] Write(QuestsDifficulty questsDifficulty)
        {
            using (BitWriter writer = new BitWriter())
            {
                Type questsDifficultyType = typeof(QuestsDifficulty);
                foreach (var questsDifficultyProperty in questsDifficultyType.GetProperties())
                {
                    Type type = questsDifficultyProperty.PropertyType;
                    var quests = questsDifficultyProperty.GetValue(questsDifficulty);
                    foreach (var property in type.GetProperties())
                    {
                        Quest quest = (Quest)property.GetValue(quests);
                        writer.WriteBytes(Quest.Write(quest));
                    }
                }
                Debug.Assert(writer.Position == 96 * 8);
                return writer.ToArray();
            }
        }

    }

  
    public class Quest
    {
        public bool RewardGranted { get; set; }
        public bool RewardPending { get; set; }
        public bool Started { get; set; }
        public bool LeftTown { get; set; }
        public bool EnterArea { get; set; }
        public bool Custom1 { get; set; }
        public bool Custom2 { get; set; }
        public bool Custom3 { get; set; }
        public bool Custom4 { get; set; }
        public bool Custom5 { get; set; }
        public bool Custom6 { get; set; }
        public bool Custom7 { get; set; }
        public bool QuestLog { get; set; }
        public bool PrimaryGoalAchieved { get; set; }
        public bool CompletedNow { get; set; }
        public bool CompletedBefore { get; set; }

        public static Quest Read(byte[] bytes)
        {
            BitArray bits = new BitArray(bytes);
            Quest quest = new Quest();
            int i = 0;
            foreach (var questProperty in typeof(Quest).GetProperties())
            {
                questProperty.SetValue(quest, bits[i++]);
            }
            return quest;
        }

        public static byte[] Write(Quest quest)
        {
            using (BitWriter writer = new BitWriter())
            {
                UInt16 flags = 0x0;
                UInt16 i = 1;
                foreach (var questProperty in typeof(Quest).GetProperties())
                {
                    if((bool)questProperty.GetValue(quest))
                    {
                        flags |= i;
                    }
                    i <<= 1;
                }
                writer.WriteUInt16(flags);
                return writer.ToArray();
            }
        }
    }

    public class ActIQuests
    {
        public Quest Introduction { get; set; }
        public Quest DenOfEvil { get; set; }
        public Quest SistersBurialGrounds { get; set; }
        public Quest ToolsOfTheTrade { get; set; }
        public Quest TheSearchForCain { get; set; }
        public Quest TheForgottenTower { get; set; }
        public Quest SistersToTheSlaughter { get; set; }
        public Quest Completion { get; set; }
    }

    public class ActIIQuests
    {
        public Quest Introduction { get; set; }
        public Quest RadamentsLair { get; set; }
        public Quest TheHoradricStaff { get; set; }
        public Quest TaintedSun { get; set; }
        public Quest ArcaneSanctuary { get; set; }
        public Quest TheSummoner { get; set; }
        public Quest TheSevenTombs { get; set; }
        public Quest Completion { get; set; }
    }

    public class ActIIIQuests
    {
        public Quest Introduction { get; set; }
        public Quest LamEsensTome { get; set; }
        public Quest KhalimsWill { get; set; }
        public Quest BladeOfTheOldReligion { get; set; }
        public Quest TheGoldenBird { get; set; }
        public Quest TheBlackenedTemple { get; set; }
        public Quest TheGuardian { get; set; }
        public Quest Completion { get; set; }
    }

    public class ActIVQuests
    {
        public Quest Introduction { get; set; }
        public Quest TheFallenAngel { get; set; }
        public Quest TerrorsEnd { get; set; }
        public Quest Hellforge { get; set; }
        public Quest Completion { get; set; }

        //3 shorts at the end of ActIV completion. presumably for extra quests never used.
        public Quest Extra1 { get; set; }
        public Quest Extra2 { get; set; }
        public Quest Extra3 { get; set; }
    }

    public class ActVQuests
    {
        public Quest Introduction { get; set; }
        //2 shorts after ActV introduction. presumably for extra quests never used.
        public Quest Extra1 { get; set; }
        public Quest Extra2 { get; set; }
        public Quest SiegeOnHarrogath { get; set; }
        public Quest RescueOnMountArreat { get; set; }
        public Quest PrisonOfIce { get; set; }
        public Quest BetrayalOfHarrogath { get; set; }
        public Quest RiteOfPassage { get; set; }
        public Quest EveOfDestruction { get; set; }
        public Quest Completion { get; set; }
        //6 shorts after ActV completion. presumably for extra quests never used.
        public Quest Extra3 { get; set; }
        public Quest Extra4 { get; set; }
        public Quest Extra5 { get; set; }
        public Quest Extra6 { get; set; }
        public Quest Extra7 { get; set; }
        public Quest Extra8 { get; set; }
    }
}
