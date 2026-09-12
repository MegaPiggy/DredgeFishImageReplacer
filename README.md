# DREDGE Fish Image Replacer

A configurable accessibility mod for **DREDGE** that replaces selected fish and creature images with simpler alternatives.

## Features

* Replace individual fish and creature images.
* Replacement images use generic shapes based on the item's dimensions in the inventory grid.
* Replacement images keep the original creature's general color.
* Supports creatures from the base game, **The Pale Reach**, and **The Iron Rig**.
* Includes presets for quickly replacing groups of creatures.
* Changes can be configured from the in-game mod settings.

## Presets

The following presets are available:

* **None** — Replace nothing.
* **All** — Replace everything.
* **Aberrations** — Replace all aberrations.
* **Sharks** — Replace sharks and their aberrations.
* **Eels** — Replace eels and their aberrations.
* **Crustaceans & Arthropods** — Replace crustaceans, arthropods, and their aberrations.
* **Deep-Sea Creatures** — Replace deep-sea creatures and their aberrations.
* **Prehistoric Creatures** — Replace prehistoric creatures and their aberrations.
* **Cephalopods** — Replace cephalopods and their aberrations.
* **Long-Legged Creatures** — Replace creatures with prominent long legs.
* **Prominent Teeth & Mouths** — Replace creatures with prominent teeth or mouths.
* **Borderline Monster** — Replace creatures that fall between ordinary fish and fully monstrous designs.

Presets are only a starting point. Individual creatures can still be enabled or disabled afterward.

If there are any fish missing from these presets that should be included, you can either:

* [Open an issue on GitHub](https://github.com/MegaPiggy/DredgeFishImageReplacer/issues)
* Ping `@megapiggy` on the [Modding Discord](https://discord.gg/qFqPuTUAmD)
* Email [megapiggy@outerwildsmods.com](mailto:megapiggy@outerwildsmods.com)

Suggestions for new presets are also welcome.

## Configuration

Open the **Fish Image Replacer** section of the Winch mod settings.

Each supported creature has its own toggle. Fish are grouped with their aberrations and separated by the base game and DLC they belong to.

Preset buttons are available at the top of the configuration menu.

## Custom Replacement Images

Replacement images can be changed to whatever you want.

They are located in the mod's folder at:

```text
Assets/Textures/
```

Generated replacement images are named after their fish ID, for example:

```text
megapiggy.fishimagereplacer.cod.png
megapiggy.fishimagereplacer.anglerfish.png
megapiggy.fishimagereplacer.cod-ab-1.png
```

Aberrations use their own fish IDs, such as `cod-ab-1`, so their replacement images can be customized separately from the normal fish.

Each replacement is generated to match the item's dimensions and occupied cells in the inventory grid. When replacing one with your own image, keep the artwork within the shape and size of the existing replacement image so it fits correctly in the item's grid.

The image itself can contain anything as long as it fits within that shape. Nothing prevents you from drawing outside it, but it may look strange in-game.

## Compatibility

Modded fish are not supported.
