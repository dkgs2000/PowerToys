// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Gilad.PowerToys.Run.Plugin.DuckDuckGo.Interfaces
{
    public interface IUriParser
    {
        bool TryParse(string input, out System.Uri result);
    }
}