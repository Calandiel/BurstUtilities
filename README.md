# Deprecation notice

This library was developed for use with Unity's Burst Compiler. At the time of writing, there were no libraries for many basic functionalities, such as lists or dictionaries without built-in references that would make them impossible to nest, as per Burst's limitations on C#. However, they've been since reimplemented by Unity. I'd suggest using their versions as they're actively supported. Look for them in the Unity.Collections package, in the LowLevel namespace.
