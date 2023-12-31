﻿using System.Diagnostics.CodeAnalysis;

namespace Mapster;

[SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "<Pending>")]
public class PreCompileArgument
{
    public Type SourceType;
    public Type DestinationType;
    public MapType MapType;
    public bool ExplicitMapping;
}