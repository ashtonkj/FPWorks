﻿namespace InfinityRpg
open OpenTK
open Nu
open Nu.Constants
open Nu.WorldConstants
module Constants =

    // misc constants
    let TileSizeI = Vector2i 64
    let TileSize = let t = TileSizeI in t.Vector2
    let TileSheetSizeM = Vector2i 4
    let TileSheetSizeI = Vector2i.Multiply (TileSheetSizeM, TileSizeI)
    let TileSheetSize = let t = TileSheetSizeI in t.Vector2
    let CharacterDepth = 1.0f
    let CharacterWalkSpeed = 4.0f

    // file paths
    let TitleGroupFilePath = "Assets/Gui/Title.nugroup"
    let CreditsGroupFilePath = "Assets/Gui/Credits.nugroup"
    let HudFilePath = "Assets/Gui/Hud.nugroup"
    let SaveFilePath = "InfinityRpg.sav"

    // package constants
    let GuiPackageName = "Gui"
    let GameplayPackageName = "Gameplay"

    // dissolve constants
    let DissolveData =
        { IncomingTime = 20L
          OutgoingTime = 30L
          DissolveImage = { PackageName = GuiPackageName; AssetName = "Dissolve" }}

    // splash constants
    let NuSplashName = "Splash"
    let NuSplashAddress = ntoa<Screen> NuSplashName
    let NuSplashData =
        { DissolveData = DissolveData
          IdlingTime = 60L
          SplashImage = { PackageName = GuiPackageName; AssetName = "Nu" }}

    // asset constants
    let PlayerImage = { PackageName = GameplayPackageName; AssetName = "Player" }
    let GoopyImage = { PackageName = GameplayPackageName; AssetName = "Goopy" }
    let BatsyImage = { PackageName = GameplayPackageName; AssetName = "Batsy" }
    let ZommieImage = { PackageName = GameplayPackageName; AssetName = "Zommie" }
    let FieldTileSheetImage = { PackageName = GameplayPackageName; AssetName = "FieldTileSheet" }

    // title constants
    let TitleName = "Title"
    let TitleAddress = ntoa<Screen> TitleName
    let TitleGroupName = DefaultGroupName
    let TitleGroupAddress = satoga TitleAddress TitleGroupName
    let ClickTitleNewGameEventAddress = ClickEventAddress ->>- TitleGroupAddress ->- ntoa "NewGame"
    let ClickTitleLoadGameEventAddress = ClickEventAddress ->>- TitleGroupAddress ->- ntoa "LoadGame"
    let ClickTitleCreditsEventAddress = ClickEventAddress ->>- TitleGroupAddress ->- ntoa "Credits"
    let ClickTitleExitEventAddress = ClickEventAddress ->>- TitleGroupAddress ->- ntoa "Exit"

    // credits constants
    let CreditsName = "Credits"
    let CreditsAddress = ntoa<Screen> CreditsName
    let CreditsGroupName = DefaultGroupName
    let CreditsGroupAddress = satoga CreditsAddress CreditsGroupName
    let ClickCreditsBackEventAddress = ClickEventAddress ->>- CreditsGroupAddress ->- ntoa "Back"

    // gameplay constants
    let GameplayName = "Gameplay"
    let GameplayAddress = ntoa<Screen> GameplayName
    
    // hud constants
    let HudName = "Hud"
    let HudAddress = satoga GameplayAddress HudName
    let HudBackName = "Back"
    let HudBackAddress = gatoea HudAddress HudBackName
    let HudSaveGameName = "SaveGame"
    let HudHaltName = "Halt"
    let HudFeelerName = "Feeler"
    let HudDetailUpName = "DetailUp"
    let HudDetailRightName = "DetailRight"
    let HudDetailDownName = "DetailDown"
    let HudDetailLeftName = "DetailLeft"
    let SceneName = "Scene"
    
    // scene constants
    let SceneAddress = satoga GameplayAddress SceneName
    let FieldName = "Field"
    let PlayerName = "Player"

    // data
    let AttackName = "Attack"
    let CharacterAnimationFacingStutter = 16L
    let CharacterAnimationActingStutter = 24L
    let ActionTicksMax = CharacterAnimationActingStutter * 3L