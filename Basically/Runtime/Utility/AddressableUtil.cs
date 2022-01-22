#if ADDRESSABLES

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

public static class AddressableUtil {
    /// <summary>
    /// Gets all entries. (Assets and their folders)
    /// </summary>
    /// <param name="locator">Default to zero.</param>
    /// <returns>All entry locations.</returns>
    public static string[] GetAllEntries(int locator = 0) {
        var map = (ResourceLocationMap)Addressables.ResourceLocators.ElementAt(locator);
        return map.Locations.Values.Select(x => x.ToString()).ToArray();
    }
}

#endif