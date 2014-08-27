﻿namespace Nu
open System
open System.Xml
open Prime
open Nu.NuConstants

[<AutoOpen>]
module AssetsModule =

    /// Describes a game asset, such as a texture, sound, or model in detail.
    /// A serializable value type.
    type [<StructuralEquality; NoComparison>] Asset =
        { Name : string
          FileName : string
          Associations : string list
          PackageName : string }

    /// All assets must belong to an asset Package, which is a unit of asset loading.
    ///
    /// In order for the renderer to render a single texture, that texture, along with all the other
    /// assets in the corresponding package, must be loaded. Also, the only way to unload any of those
    /// assets is to send an AssetPackageUnload message to the renderer, which unloads them all. There
    /// is an AssetPackageLoad message to load a package when convenient.
    ///
    /// The use of a message system for the renderer should enable streamed loading, optionally with
    /// smooth fading-in of late-loaded assets (IE - assets that are already in the view frustum but are
    /// still being loaded).
    ///
    /// Finally, the use of AssetPackages could enforce assets to be loaded in order of size and will
    /// avoid unnecessary Large Object Heap fragmentation.
    ///
    /// A serializable value type.
    type [<StructuralEquality; NoComparison>] Package =
        { Name : string
          AssetNames : string list }

    type [<StructuralEquality; NoComparison>] AssetKey =
        { Name : string
          PackageName : string }

    type 'a AssetMap = Map<string, Map<string, 'a>>

[<RequireQualifiedAccess>]
module Assets =

    let tryLoadAssetFromAssetNode (node : XmlNode) =
        let attributes = node.Attributes
        let optName = attributes.GetNamedItem NameAttributeName
        match optName with
        | null -> None
        | name ->
            let optFileName = attributes.GetNamedItem FileNameAttributeName
            match optFileName with
            | null -> None
            | fileName ->
                let optAssociations = attributes.GetNamedItem AssociationsAttributeName
                match optAssociations with
                | null -> None
                | associations ->
                    let associationList = List.ofArray (associations.InnerText.Split ',')
                    Some {
                        Name = name.InnerText
                        FileName = fileName.InnerText
                        Associations = associationList
                        PackageName = node.ParentNode.Name }

    let tryLoadAssetsFromPackageNode optAssociation (node : XmlNode) =
        let assets =
            List.fold
                (fun assets assetNode ->
                    match tryLoadAssetFromAssetNode assetNode with
                    | None ->
                        debug <| "Invalid asset node in '" + node.Name + "' in asset graph."
                        assets
                    | Some asset -> asset :: assets)
                []
                (List.ofSeq <| enumerable node.ChildNodes)
        let associatedAssets =
            match optAssociation with
            | None -> assets
            | Some association ->
                List.filter
                    (fun asset -> List.exists ((=) association) asset.Associations)
                    assets
        List.ofSeq associatedAssets
        
    let tryLoadAssetsFromRootNode optAssociation (node : XmlNode) =
        let possiblePackageNodes = List.ofSeq <| enumerable node.ChildNodes
        let packageNodes =
            List.fold
                (fun packageNodes (node : XmlNode) ->
                    if node.Name = PackageNodeName then node :: packageNodes
                    else packageNodes)
                []
                possiblePackageNodes
        let assetLists =
            List.fold
                (fun assetLists packageNode ->
                    let assets = tryLoadAssetsFromPackageNode optAssociation packageNode
                    assets :: assetLists)
                []
                packageNodes
        let assets = List.concat assetLists
        Right assets

    // TODO: test this function!
    let tryLoadAssetsFromPackageNodes optAssociation nodes =
        let packageNames = List.map (fun (node : XmlNode) -> node.Name) nodes
        match packageNames with
        | [] ->
            Left <|
                "Multiple packages have the same names '" +
                String.Join ("; ", packageNames) +
                "' which is an error."
        | _ :: _ ->
            let eitherPackageAssetLists =
                List.map
                    (fun (node : XmlNode) ->
                        match tryLoadAssetsFromRootNode optAssociation node with
                        | Left error -> Left error
                        | Right assets -> Right (node.Name, assets))
                    nodes
            let (errors, assets) = Either.split eitherPackageAssetLists
            match errors with
            | [] -> Right <| Map.ofList assets
            | _ :: _ -> Left <| "Error(s) when loading assets '" + String.Join ("; ", errors) + "'."

    let tryLoadAssetsFromPackage optAssociation packageName (assetGraphFileName : string) =
        try let document = XmlDocument ()
            document.Load assetGraphFileName
            let optRootNode = document.[RootNodeName]
            match optRootNode with
            | null -> Left "Root node is missing from asset graph."
            | rootNode ->
                let possiblePackageNodes = List.ofSeq <| enumerable rootNode.ChildNodes
                let packageNodes =
                    List.filter
                        (fun (node : XmlNode) ->
                            node.Name = PackageNodeName &&
                            (node.Attributes.GetNamedItem NameAttributeName).InnerText = packageName)
                        possiblePackageNodes
                match packageNodes with
                | [] -> Left <| "Package node '" + packageName + "' is missing from asset graph."
                | [packageNode] -> Right <| tryLoadAssetsFromPackageNode optAssociation packageNode
                | _ :: _ -> Left <| "Multiple packages with the same name '" + packageName + "' is an error."
        with exn -> Left <| string exn

    // TODO: test this function!
    let tryLoadAssetsFromPackages optAssociation packageNames (assetGraphFileName : string) =
        try let document = XmlDocument ()
            document.Load assetGraphFileName
            let optRootNode = document.[RootNodeName]
            match optRootNode with
            | null -> Left "Root node is missing from asset graph."
            | rootNode ->
                let possiblePackageNodes = List.ofSeq <| enumerable rootNode.ChildNodes
                let packageNameSet = Set.ofList packageNames
                let packageNodes =
                    List.filter
                        (fun (node : XmlNode) ->
                            node.Name = PackageNodeName &&
                            Set.contains (node.Attributes.GetNamedItem NameAttributeName).InnerText packageNameSet)
                        possiblePackageNodes
                tryLoadAssetsFromPackageNodes optAssociation packageNodes
        with exn -> Left <| string exn

    let tryLoadAssetFromDocument optAssociation (assetGraphFileName : string) =
        try let document = XmlDocument ()
            document.Load assetGraphFileName
            let optRootNode = document.[RootNodeName]
            match optRootNode with
            | null -> Left "Root node is missing from asset graph."
            | rootNode -> tryLoadAssetsFromRootNode optAssociation rootNode
        with exn -> Left <| string exn