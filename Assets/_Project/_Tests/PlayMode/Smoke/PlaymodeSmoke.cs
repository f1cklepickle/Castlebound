using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class PlaymodeSmoke
{
    [UnityTest]
    public IEnumerator PassesNextFrame()
    {
        yield return null;
        Assert.IsTrue(true);
    }
}

