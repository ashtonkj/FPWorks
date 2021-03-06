﻿namespace Nu
open System
open Xunit
open Prime
open Prime.Desync
open Nu
open Nu.Constants
open Nu.WorldConstants
open Nu.Observation
open Nu.Desync
module DesyncTests =

    let IntEventAddress = stoa<int> "Test"
    let incUserState _ world = World.updateUserState inc world
    let incUserStateNoEvent world = World.updateUserState inc world
    let incUserStateTwice _ world = World.updateUserState (inc >> inc) world
    let incUserStateTwiceNoEvent world = World.updateUserState (inc >> inc) world

    let [<Fact>] desyncWorks () =
        
        // build everything
        let world = World.initAndMakeEmpty 0
        let desync =
            desync {
                let! e = next
                do! update <| incUserState e
                do! react incUserStateNoEvent
                do! reactE incUserState
                do! pass
                do! loop 0 inc (fun i _ -> i < 2) (fun _ -> update incUserStateTwiceNoEvent) }
        let observation = observe IntEventAddress GameAddress
        let world = snd <| Desync.runDesyncAssumingCascade desync observation world
        Assert.Equal (0, World.getUserState world)

        // assert the first publish executes the first desync'd operation
        let world = World.publish4 1 IntEventAddress GameAddress world
        Assert.Equal (1, World.getUserState world)

        // assert the second publish executes the second desync'd operation
        let world = World.publish4 2 IntEventAddress GameAddress world
        Assert.Equal (2, World.getUserState world)
        
        // and so on...
        let world = World.publish4 3 IntEventAddress GameAddress world
        Assert.Equal (3, World.getUserState world)
        
        // and so on...
        let world = World.publish4 4 IntEventAddress GameAddress world
        Assert.Equal (7, World.getUserState world)
        
        // assert no garbage is left over after desync'd computation is concluded
        Assert.True <| Map.isEmpty world.Callbacks.CallbackStates