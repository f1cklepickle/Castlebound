using System.Collections.Generic;
using UnityEngine;

public class EnemyRingManager : MonoBehaviour
{
    [SerializeField] private float bandWidth = 1.0f;     // ± distance band around median ring
    [SerializeField] private int updateEveryN = 1;       // 1 = every FixedUpdate
    [SerializeField] private float neighborArcDeg = 60f; // local neighborhood arc

    private int _tick;

    // Scratch buffers to avoid per-frame allocations
    private static EnemyController2D[] sEnemies = new EnemyController2D[64];
    private static float[] sAngles = new float[64];
    private static float[] sDists = new float[64];
    private static float[] sScratch = new float[64];

    private const float TwoPI = Mathf.PI * 2f;

    private void FixedUpdate()
    {
        int n = EnemyController2D.All != null ? EnemyController2D.All.Count : 0;
        if (n == 0) return;

        _tick++;
        int step = updateEveryN < 1 ? 1 : updateEveryN;
        if ((_tick % step) != 0) return;

        // Anchor ring spacing on the actual player, not the current chase target.
        Vector3 playerPos3 = default;
        bool foundPlayer = false;

        // Prefer the Player tag.
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerPos3 = playerGO.transform.position;
            foundPlayer = true;
        }

        // Fallback: any enemy's player reference.
        if (!foundPlayer)
        {
            for (int i = 0; i < n; i++)
            {
                EnemyController2D e = EnemyController2D.All[i];
                if (e != null && e.Target != null && e.Target.CompareTag("Player"))
                {
                    playerPos3 = e.Target.position;
                    foundPlayer = true;
                    break;
                }
            }
        }

        if (!foundPlayer) return;

        Vector2 playerPos = new Vector2(playerPos3.x, playerPos3.y);

        EnsureCapacity(n);

        // Build arrays: enemies, angles, dists
        int count = 0;
        for (int i = 0; i < n; i++)
        {
            EnemyController2D e = EnemyController2D.All[i];
            if (e == null) continue;

            Vector3 p = e.transform.position;
            float dx = p.x - playerPos.x;
            float dy = p.y - playerPos.y;

            sEnemies[count] = e;
            sAngles[count] = Mathf.Atan2(dy, dx);    // [-pi, pi]
            sDists[count] = Mathf.Sqrt(dx * dx + dy * dy);
            count++;
        }

        if (count == 0) return;

        // Sort by angle (ascending), keeping arrays in sync
        QuickSortByKey(sAngles, sEnemies, sDists, 0, count - 1);

        // Compute distance median using quickselect on scratch buffer
        CopyArray(sDists, sScratch, count);
        float median = ComputeMedianInPlace(sScratch, count);

        // Threshold for local neighborhood
        float neighborArcRad = neighborArcDeg * Mathf.Deg2Rad;

        // Feed angular gaps to each enemy
        if (count == 1)
        {
            // Single enemy: no meaningful neighbors; send zeros
            sEnemies[0].SetAngularGaps(0f, 0f);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            int cw = (i - 1 + count) % count;
            int ccw = (i + 1) % count;

            float a_i = sAngles[i];
            float a_cw = sAngles[cw];
            float a_ccw = sAngles[ccw];

            // Positive angular gaps
            float gapCW = a_i - a_cw;
            if (gapCW < 0f) gapCW += TwoPI;

            float gapCCW = a_ccw - a_i;
            if (gapCCW < 0f) gapCCW += TwoPI;

            // Filter out non-local gaps
            if (gapCW > neighborArcRad) gapCW = 0f;
            if (gapCCW > neighborArcRad) gapCCW = 0f;

            // Band filter around median ring
            float dist = sDists[i];
            bool inBand = Mathf.Abs(dist - median) <= bandWidth;
            if (!inBand)
            {
                gapCW = 0f;
                gapCCW = 0f;
            }

            sEnemies[i].SetAngularGaps(gapCW, gapCCW);
        }
    }

    private static void EnsureCapacity(int needed)
    {
        if (sEnemies.Length < needed)
        {
            int newSize = NextSize(sEnemies.Length, needed);
            System.Array.Resize(ref sEnemies, newSize);
            System.Array.Resize(ref sAngles, newSize);
            System.Array.Resize(ref sDists, newSize);
        }
        if (sScratch.Length < needed)
        {
            int newSize = NextSize(sScratch.Length, needed);
            System.Array.Resize(ref sScratch, newSize);
        }
    }

    private static int NextSize(int current, int needed)
    {
        if (current < 1) current = 64;
        while (current < needed) current <<= 1;
        return current;
    }

    private static void CopyArray(float[] src, float[] dst, int count)
    {
        for (int i = 0; i < count; i++) dst[i] = src[i];
    }

    // In-place quicksort on parallel arrays keyed by keys[]
    private static void QuickSortByKey(float[] keys, EnemyController2D[] a1, float[] a2, int left, int right)
    {
        int i = left, j = right;
        float pivot = keys[(left + right) >> 1];

        while (i <= j)
        {
            while (keys[i] < pivot) i++;
            while (keys[j] > pivot) j--;
            if (i <= j)
            {
                // swap i, j for all arrays
                float tk = keys[i]; keys[i] = keys[j]; keys[j] = tk;

                EnemyController2D t1 = a1[i]; a1[i] = a1[j]; a1[j] = t1;

                float t2 = a2[i]; a2[i] = a2[j]; a2[j] = t2;

                i++; j--;
            }
        }
        if (left < j) QuickSortByKey(keys, a1, a2, left, j);
        if (i < right) QuickSortByKey(keys, a1, a2, i, right);
    }

    // Median via QuickSelect on scratch buffer (does not allocate)
    private static float ComputeMedianInPlace(float[] arr, int len)
    {
        if (len <= 0) return 0f;
        int mid = len >> 1;
        if ((len & 1) == 1)
        {
            return QuickSelect(arr, 0, len - 1, mid);
        }
        else
        {
            float lo = QuickSelect(arr, 0, len - 1, mid - 1);
            float hi = QuickSelect(arr, 0, len - 1, mid);
            return 0.5f * (lo + hi);
        }
    }

    // k-th smallest (0-based) using Hoare partition scheme
    private static float QuickSelect(float[] arr, int left, int right, int k)
    {
        while (true)
        {
            if (left == right) return arr[left];

            int i = left, j = right;
            float pivot = arr[(left + right) >> 1];

            while (i <= j)
            {
                while (arr[i] < pivot) i++;
                while (arr[j] > pivot) j--;
                if (i <= j)
                {
                    float t = arr[i]; arr[i] = arr[j]; arr[j] = t;
                    i++; j--;
                }
            }

            if (k <= j) right = j;
            else if (k >= i) left = i;
            else return arr[k];
        }
    }
}
