# Otsimo Coloring Game

A simple Unity coloring game prototype built for the Otsimo software engineering intern study.

## Overview

This project contains a small children’s coloring app flow with:

- a `MainScene` with a level selection grid
- a `ColoringScene` where the player paints the selected image
- 3 playable coloring levels
- a 6-color palette and an eraser
- completion tracking and local progress saving

## Built With

- Unity `2022.3 LTS`
- C#
- DOTween for UI animations

## How To Run

1. Open the project in Unity `2022.3 LTS`
2. Open `Assets/Scenes/MainScene.unity`
3. Press Play in the Unity Editor
4. Select a level from the grid
5. Pick a color and tap a region to fill it

## Gameplay Flow

### Main Scene

- shows the available levels
- lets the player choose a level
- shows completion marks for finished levels

### Coloring Scene

- loads the selected line art image
- lets the player fill connected regions
- keeps the fill inside the original outlines
- shows a completion panel when the painted area passes 90%
- lets the player return to the level grid

## Features

- flood fill painting based on the tapped region color
- outline detection using the original source texture
- persistent selected color
- persistent level completion state
- persistent in-progress canvas state per level
- completion panel animation with DOTween
- editor menu tool to reset all saved progress

## Save System

The project uses two kinds of local save data:

- `PlayerPrefs` for selected color and completed level state
- raw texture files for each level canvas in `Application.persistentDataPath`

This means the player can leave a level and come back without losing the current coloring progress.

## Reset Progress

The project includes a Unity Editor menu item:

- `Tools/Reset PlayerPrefs`

This clears both:

- saved `PlayerPrefs`
- saved level paint files

## Project Structure

### Scenes

- `Assets/Scenes/MainScene.unity`
- `Assets/Scenes/ColoringScene.unity`

### Main Scripts

- `Assets/Scripts/PaintOnClick.cs`
  handles painting, fill logic, completion check, and saving
- `Assets/Scripts/ColorPicker.cs`
  handles color selection and swatch visuals
- `Assets/Scripts/EraserButton.cs`
  switches the tool into erase mode
- `Assets/Scripts/LevelButton.cs`
  stores the selected level and opens the coloring scene
- `Assets/Scripts/LevelCompletionView.cs`
  shows completion marks in the level grid
- `Assets/Scripts/LevelProgressStorage.cs`
  saves and loads per-level canvas files
- `Assets/Editor/ResetProgress.cs`
  clears saved data from the editor menu

## Notes

- the project is designed to be playable in the Unity Editor
- DOTween is already included in the project
- the current setup is focused on portrait mobile-style gameplay
