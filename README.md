# Prey Predator

Add this folder in Unity Hub. In Unity, open `Assets/Scenes/SampleScene.unity` and press **Play**.

The scene includes a ground plane with a material applied, a light, and a free camera. The models are in **Assets/Models**, and the ground material is in **Assets/Materials**. The wolf's animation clips are included in **Wolf.fbx**. You will create the prefabs, animation controllers, and animal behaviors.

Press **F2** to open the population graph. It stays empty until it receives data from the simulation. Call `plot.RecordPopulation(sheep, wolves, grass)` in your script whenever you record the population counts.

## Speeding up the simulation

Add the **Simulation Speed** component to the GameObject that runs your simulation. In the **Game** view, use the first four keys on the number row or the corresponding keys on the numeric keypad. **1 → ×1**, **2 → ×2**, **3 → ×4**, **4 → ×8**. The HUD displays the current simulation speed, and the camera moves at its usual speed.

## Model credits

The source files credit Grass Tuft to Vice Cooper, Heart to Google, Fork to Poly by Google, and Duck to Alexander Brodin.
