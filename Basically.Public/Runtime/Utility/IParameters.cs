using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Utility {
    public interface IParameters {
        void Add<T>(string name, T value) where T : struct;
        T Get<T>(string name) where T : struct;
    }
}
