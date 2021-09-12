using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Basically.Public.Editor")]
#if BASICALLY_INSTALLED
[assembly: InternalsVisibleTo("Basically")]
[assembly: InternalsVisibleTo("Basically.Editor")]
[assembly: InternalsVisibleTo("Basically.Editor.Tests")]
#endif
