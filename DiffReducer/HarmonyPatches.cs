//using CustomJSONData.CustomBeatmap;
using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using static PlayerSaveData;
using Zenject;
using UnityEngine;
using DiffReducer.UI;
namespace DiffReducer
{

    #region 2 Postfix - CreateTransformedBeatmapData
    //BW runs after StandardLevelDetailView. This alters the beat map data
    //BW Log statements show that 360fyer creates __result before this runs.
    [HarmonyPatch(typeof(BeatmapDataTransformHelper))]
    [HarmonyPatch("CreateTransformedBeatmapData")]
    class DiffReductionPatch
    {
        static void Postfix(IReadonlyBeatmapData beatmapData, ref IReadonlyBeatmapData __result, bool leftHanded)
        {
            if (!BS_Utils.Plugin.LevelData.IsSet || !UI.ModifierUI.instance.modEnabled || BS_Utils.Plugin.LevelData.Mode != BS_Utils.Gameplay.Mode.Standard)//not multiplayer or mission etc
                return;

            if (TransitionPatcher.originalNPS < ModifierUI.instance.DisableBelowThisNPS)
            {
                Plugin.log.Info($"DISABLED for this level since originalNPS: {TransitionPatcher.originalNPS} is less than {ModifierUI.instance.DisableBelowThisNPS}");
                return;
            }
            else
                Plugin.log.Info($"ENABLED for this level since originalNPS: {TransitionPatcher.originalNPS} is equal or greater than {ModifierUI.instance.DisableBelowThisNPS}");

            BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("DiffReducer");

            __result = __result.GetFilteredCopy(x =>
            {
                if (x is BeatmapObjectData && !UI.ModifierUI.instance.simplifiedMap.Any(y => x.CompareTo(y) == 0))
                    return null;
                return x;
            });//BW __result = beatmapData.GetFilteredCopy(x =>... was the original statement. It now works with 360fyer using__result = __result.GetFilteredCopy(x =>
        }
    }
    #endregion

    #region 1 Prefix - StartStandardLevel --BW added this so can decide if want to disable plugin automatically based on original NPS
    //BW 2nd item that runs after StandardLevelDetailView
    //Runs when you click play button
    //I think this requires colors.dll - got errors in BS logs. i put several dlls in but it started to work with colors.dll
    [HarmonyPatch(typeof(MenuTransitionsHelper))]
    [HarmonyPatch("StartStandardLevel", new[] { typeof(string), typeof(IDifficultyBeatmap), typeof(IPreviewBeatmapLevel), typeof(OverrideEnvironmentSettings), typeof(ColorScheme), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(string), typeof(bool), typeof(bool), typeof(Action), typeof(Action<DiContainer>), typeof(Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>), typeof(Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>), typeof(RecordingToolManager.SetupData) })]//v1.34 added last item
    public class TransitionPatcher
    {
        public static float originalNPS;

        static void Prefix(string gameMode, IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel previewBeatmapLevel, OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, string backButtonText, bool useTestNoteCutSoundEffects, bool startPaused, Action beforeSceneSwitchCallback, Action<DiContainer> afterSceneSwitchCallback, Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> levelFinishedCallback, Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults> levelRestartedCallback)
        {
            string startingGameMode = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;

            IReadonlyBeatmapData beatmapData = RetrieveBeatmapData(difficultyBeatmap, previewBeatmapLevel.environmentInfo, playerSpecificSettings);

            IReadonlyBeatmapData RetrieveBeatmapData(IDifficultyBeatmap theDifficultyBeatmap, EnvironmentInfoSO environmentInfo, PlayerSpecificSettings thePlayerSpecificSettings)
            {
                IReadonlyBeatmapData theBeatmapData = Task.Run(() => difficultyBeatmap.GetBeatmapDataAsync(environmentInfo, playerSpecificSettings)).Result;
                //Plugin.Log.Info($"PlayerSpecificSettings - NoteJumpDurationTypeSettings: {playerSpecificSettings.noteJumpDurationTypeSettings}. if Static - noteJumpFixedDuration(reaction time): {playerSpecificSettings.noteJumpFixedDuration} or if Dynamic - Note Jump Offset: {playerSpecificSettings.noteJumpStartBeatOffset}");

                return theBeatmapData;
            }

            var objects = beatmapData.allBeatmapDataItems.Where(x => x is BeatmapObjectData).Cast<BeatmapObjectData>();
            var notes = objects.Where(x => x.IsNote());
            originalNPS = objects.Count() > 0 ? notes.Count() / notes.Last().time : 0;

            Plugin.log.Info($"Songname: {difficultyBeatmap.level.songName} - {startingGameMode} - {difficultyBeatmap.difficulty.ToString()} - originalNPS: {originalNPS}");
        }
    }   
    #endregion

    /*
    //1 Called when a song's been selected and its levels are displayed in the right menu
    // it only works on the difficulty that is preselected. you cannot select another difficulty to see its information.
    [HarmonyPatch(typeof(StandardLevelDetailView))]
    [HarmonyPatch("SetContent")]
    public class LevelUpdatePatcher
    {
        public static string SongName;
        public static string Difficulty;
        public static bool BeatSage;
        //public static float SongDuration;
        public static ColorScheme OriginalColorScheme;
        public static bool AlreadyUsingEnvColorBoost;
        //public static float CuttableNotesCount;

        static void Prefix(StandardLevelDetailView __instance, IBeatmapLevel level, BeatmapDifficulty defaultDifficulty, BeatmapCharacteristicSO defaultBeatmapCharacteristic, PlayerData playerData)//level actually is of the class CustomBeatmapLevel which impliments interface IBeatmapLevel
        {

            SongName = level.songName;
            Difficulty = defaultDifficulty.ToString();

            Plugin.log.Info($"Songname: {SongName} - {Difficulty}");
        }
    }
    */
}
