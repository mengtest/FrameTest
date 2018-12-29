using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Window  {
    bool Initialize(params object[] args);
    bool Show();
    bool Close();
}
