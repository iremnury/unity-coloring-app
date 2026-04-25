For flood fill, I used a queue-based BFS approach. It is easy to reason about, works well with irregular regions, and avoids the stack overflow risk of recursive solutions.

The fill operation runs directly on the main thread, but since the texture size is small, it does not cause a noticeable freeze. If I had to support larger images, I would split the work across frames using a coroutine.

I kept the original texture unchanged and used it for outline checks. This prevents the fill from crossing borders even if a region is recolored multiple times.

I used PlayerPrefs only for small key-value data such as completed levels and the last selected color. For in-progress painting data, I stored each level canvas as a raw texture file in Application.persistentDataPath, since PlayerPrefs is not a good fit for larger pixel buffers.

Because the save system is split across PlayerPrefs and per-level files, resetting progress needs to clear both the saved files and any currently loaded in-memory canvas state.

The color palette is stored in a ScriptableObject, which makes it easier to manage and reuse color data without changing the code.

I preferred bright, vivid palette colors so the coloring experience feels more playful for kids.

For supporting UI elements such as the eraser, back button, frames, and completion mark, I used PNG assets sourced from the internet.

If the canvas size was increased to 2048x2048, the fill operation and texture updates would become significantly slower and could cause performance issues.

In that case, I would consider splitting the fill into smaller batches or optimizing how texture updates are handled.

As a small extra feature, I also added an eraser tool to improve the user experience.
