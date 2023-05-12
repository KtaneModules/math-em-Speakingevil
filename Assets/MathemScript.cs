using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class MathemScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public GameObject[] tiles;
    public GameObject[] doors;
    public GameObject doorpivot;
    public GameObject matstore;
    public List<KMSelectable> keys;
    public Renderer[] tlabels;
    public Material[] tpatterns;
    public TextMesh disp;

    private int[,] tprops = new int[16, 3];
    private int[][] tarrange = new int[6][];
    private List<long> nums = new List<long> { };
    private long ans;
    private int sub;
    private bool[] active = new bool[2] { true, false};
    private bool phasetwo;
    private bool tp;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private string Bin(long x)
    {
        if (x == 0)
            return "F";
        string b = "";
        while(x > 0)
        {
            b += "FT"[(int)(x % 2)];
            x >>= 1;
        }
        return b;
    }

    private string Balter(long x)
    {
        if (x == 0)
            return "0";
        string b = "";
        while(x > 0)
        {
            int r = (int)(x % 3);
            if (r == 2)
                x++;
            x /= 3;
            b += "0+-"[r];
        }
        return b;
    }

    private long Sqr(long x)
    {
        long r = x / 2;
        long q = 0;
        while (r != q)
        {
            q = r;
            r = (r + (x / r)) / 2;
        }
        return r;
    }

    private long Op(long a, long b, int k)
    {
        if (b > a)
        {
            long t = a;
            a = b;
            b = t;
        }
        switch (k)
        {
            case 1: return a + b;
            case 2: return 2 * Sqr((a * a) + (b * b));
            case 3: return a - b;
            case 4: string[] bter = new string[2] { Balter(a), Balter(b) };
                long btprod = 0;
                long btrit = 1;
                for(int i = 0; i < bter[1].Length; i++)
                {
                    if (bter[0][i] != '0' && bter[1][i] != '0')
                        if (bter[0][i] == bter[1][i])
                            btprod += btrit;
                        else
                            btprod -= btrit;
                    btrit *= 3;
                }
                return btprod < 0 ? -btprod : btprod;
            case 6: return Sqr(Math.Abs((b * b) - ((a - b) * (a - b))));
            case 7: return b == 0 ? 0 : a % b;
            case 8: double[] t = new double[4] { Math.Log10(a), Math.Log10(b), Math.Log10((a + b) / 2), 0};
                t[3] = t[0] + t[1] - t[2];
                return (long)Math.Pow(10, t[3]);
            case 10: int[] l = new int[2] { (int)Math.Log10(a) + 1, (int)Math.Log10(b) + 1};
                return (l[0] * l[0]) + (l[1] * l[1]);
            case 14: return b == 0 ? 0 : (a % b == 0 ? 0 : b % (a % b));
            default: string[] bin = new string[2] { Bin(a), Bin(b)};            
                long output = 0;
                string xor = "";
                long d = 1;
                if (k == 22)
                    return bin[0].Count(x => x == 'T') * bin[1].Count(x => x == 'T');
                for (int i = 0; i < bin[0].Length; i++)
                {
                    switch (k)
                    {
                        case 15:
                            if (bin[0][i] == 'F' ^ (i >= bin[1].Length || bin[1][i] == 'F'))
                                output += d;
                            break;
                        case 16:
                            if (bin[0][i] == 'T' || (i < bin[1].Length && bin[1][i] == 'T'))
                                output += d;
                            break;
                        case 18:
                            if (i < bin[1].Length && bin[0][i] == 'T' && bin[1][i] == 'T')
                                output += d;
                            break;
                        default:
                            if (bin[0][i] == 'F' ^ (i >= bin[1].Length || bin[1][i] == 'F'))
                                xor += 'T';
                            else
                                xor += 'F';
                            break;
                    }
                    if(k != 30)
                        d *= 2;
                }
                if(k != 30)
                    return output;
                int z = xor.Length;
                d = 1;
                for(int i = 0; i < z; i++)
                {
                    if (xor[(i + 1) % z] == 'T' ^ (xor[i] == 'T' || xor[(i + z - 1) % z] == 'T'))
                        output += d;
                    d *= 2;
                }
                return output;
        }
    }

    private int Eval(int i, int x, int s)
    {
        switch (x)
        {
            case 0: return (Enumerable.Range(0, 16).Count(k => tprops[k, 1] == s) - 1) % 10;
            case 1:
                for (int j = i + 1; j < i + 16; j++)
                {
                    int k = j % 16;
                    if (tprops[tarrange[5][k], 0] == 0 && tprops[tarrange[5][k], 1] == s)
                        return tprops[tarrange[5][k], 2];
                }
                return 0;
            case 2:
                bool fl = false;
                switch (s)
                {
                    case 0: fl = i <= 7; break;
                    case 1: fl = (i / 4) % 2 == 0; break;
                    case 2: fl = (i / 2) % 2 == 0; break;
                    case 3: fl = i % 2 == 0; break;
                }
                return fl ? info.GetSerialNumberNumbers().First() : info.GetSerialNumberNumbers().Last();
            case 3:
                switch (s)
                {
                    case 0: return (i + 1) % 10;
                    case 1: return (((3 - (i % 4)) * 4 + (i / 4)) + 1) % 10;
                    case 2: return (((3 - (i / 4)) * 4 + (i % 4)) + 1) % 10;
                    default: return (((i % 4) * 4 + (3 - (i / 4))) + 1) % 10;
                }
            case 4:
                int p = i / 4; int q = i % 4;
                int[] adj = new int[4] {  ((p + 1) * 4 + q) % 16, ((p + 3) * 4 + q) % 16, p * 4 + ((q + 1) % 4), p * 4 + ((q + 3) % 4)};
                return Enumerable.Range(0, 4).Select(k => tprops[tarrange[5][adj[k]], 0] == 0 ? tprops[tarrange[5][adj[k]], 2] : 0).Sum() % 10;
            case 5:
                return Enumerable.Range(0, 4).Select(k => tprops[tarrange[5][(k * 4) + s], 0] == 0 ? tprops[tarrange[5][(k * 4) + s], 2] : 0).Sum() % 10;
            case 6:
                switch (s)
                {
                    case 0: return ((int)info.GetTime() / 60) % 10;
                    case 1: return DateTime.Now.Minute % 10;
                    case 2: return DateTime.Now.Hour % 10;
                    default: return DateTime.Today.Day % 10;
                }
            case 7:
                switch (s)
                {
                    case 0: return info.GetBatteryCount() % 10;
                    case 1: return info.GetPortCount() % 10;
                    case 2: return info.GetIndicators().Count() % 10;
                    default: return info.GetIndicators().Join().Count(k => !"AEIOU ".Contains(k.ToString())) % 10;
                }
            case 8:
                int p2 = i / 4; int q2 = i % 4;
                return Enumerable.Range(0, 16).Select(k => (k % 4 == q2 || k / 4 == p2 || tprops[tarrange[5][k], 0] > 0 || tprops[tarrange[5][k], 1] == s) ? 0 : tprops[tarrange[5][k], 2]).Sum() % 10;
            default:
                return Enumerable.Range(0, 16).Select(k => (tprops[tarrange[5][k], 0] > 0 || tprops[tarrange[5][k], 2] >= info.GetSerialNumberNumbers().Max()) ? 0 : tprops[tarrange[5][k], 2]).Sum() % 10;
        }
    }

    private void Interpret()
    {
        long d = 0;
        int[] s = Enumerable.Range(0, 16).Select(k => tarrange[5][k]).ToArray();
        string[] table = Enumerable.Range(0, 16).Select(i => "WBSG"[tprops[s[i], 1]] + "0123456789ABCDEFGHIJ+-/^"[(tprops[s[i], 0] * 10) + tprops[s[i], 2]].ToString()).ToArray();
        Debug.LogFormat("[Math 'em #{0}] The arrangement of the tiles is:\n[Math 'em #{0}] {1}", moduleID, string.Join("\n[Math 'em #" + moduleID + "] ", Enumerable.Range(0, 4).Select(i => string.Join(" ", table.Where((x, j) => j / 4 == i).ToArray())).ToArray()));
        for (int i = 0; i < 16; i++)
        {
            switch(tprops[s[i], 0])
            {
                case 0:
                    if (d < 10000000)
                    {
                        d *= 10;
                        d += tprops[s[i], 2];
                    }
                    break;
                case 1:
                    if (d < 10000000)
                    {
                        d *= 10;
                        d += Eval(i, tprops[s[i], 2], tprops[s[i], 1]);
                    }
                    break;
                default:
                    if(i > 0 && i < 15 && tprops[s[i + 1], 0] < 2 && !(i == 1 && tprops[s[0], 0] == 2))
                    {
                        nums.Add(d);
                        d = 0;
                    }
                    break;
            }
        }
        nums.Add(d);
        if (tprops[s[15], 0] == 2)
        {
            nums.Add(nums[0]);
            if (tprops[s[0], 0] == 2)
                nums.Insert(0, nums[nums.Count() - 2]);
        }
        else if (tprops[s[0], 0] == 2)
            nums.Insert(0, nums.Last());
        Debug.LogFormat("[Math 'em #{0}] The arrangement yields the values: {1}", moduleID, string.Join(", ", nums.Select(k => k.ToString()).ToArray()));
        int ind = 0;
        ans = nums[0];
        nums.RemoveAt(0);
        string olog = string.Empty;
        for (int i = 0; i < 16; i++)
        {
            if (tprops[s[i], 0] < 2)
                continue;
            string alog = ans.ToString();
            int t = tprops[s[i], 2];
            if (i == 0 || tprops[s[i - 1], 0] < 2)
            {
                ind = new int[4] { 1, 3, 7, 15 }[t];
                olog = "+-/^"[t].ToString();
            }
            if (i == 15 || tprops[s[i + 1], 0] < 2)
            {
                if (i != 0 && tprops[s[i - 1], 0] == 2)
                {
                    ind += new int[4] { 1, 3, 7, 15 }[t];
                    olog += "+-/^"[t].ToString();
                }
                ans = Op(ans, nums[0], ind);
                Debug.LogFormat("[Math 'em #{0}] {1} {2} {3} = {4}", moduleID, alog, olog, nums[0], ans);
                ind = 0;
                olog = string.Empty;
                nums.RemoveAt(0);
            }
        }
        ans %= 100000000;
        Debug.LogFormat("[Math 'em #{0}] The final answer is {1}.", moduleID, ans);
    }

    private int[] Swap(int[] x)
    {
        int[] map = new int[16];
        switch(Random.Range(0, 3))
        {
            case 0:
                bool rswap = Random.Range(0, 2) == 0;
                int[] inds = new int[] { 0, 1, 2, 3}.Shuffle().Where((z, i) => i < 2).ToArray();
                Debug.LogFormat("[Math 'em #{0}] Swap {1} {2} and {3}.", moduleID, rswap ? "rows" : "columns", inds[0] + 1, inds[1] + 1);
                if (rswap)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if(i / 4 == inds[0])
                            map[i] = inds[1] * 4 + (i % 4);
                        else if (i / 4 == inds[1])
                            map[i] = inds[0] * 4 + (i % 4);
                        else
                            map[i] = i;
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (i % 4 == inds[0])
                            map[i] = (i / 4) * 4 + inds[1];
                        else if (i % 4 == inds[1])
                            map[i] = (i / 4) * 4 + inds[0];
                        else
                            map[i] = i;
                    }
                }
                break;
            case 1:
                bool[] shift = new bool[2] { Random.Range(0, 2) == 0, Random.Range(0, 2) == 0};
                int ind = Random.Range(0, 4);
                if (shift[0])
                {
                    Debug.LogFormat("[Math 'em #{0}] Shift row {1} {2}.", moduleID, ind + 1, shift[1] ? "left" : "right");
                    for(int i = 0; i < 16; i++)
                    {
                        if (i / 4 == ind)
                            map[i] = (i / 4) * 4 + (i + (shift[1] ? 1 : 3)) % 4;
                        else
                            map[i] = i;
                    }
                }
                else
                {
                    Debug.LogFormat("[Math 'em #{0}] Shift column {1} {2}.", moduleID, ind + 1, shift[1] ? "up" : "down");
                    for (int i = 0; i < 16; i++)
                    {
                        if (i % 4 == ind)
                            map[i] = (i + (shift[1] ? 1 : 3) * 4) % 16;
                        else
                            map[i] = i;
                    }
                }
                break;
            default:
                int[] cycle = new int[4] { Random.Range(0, 16), 0, Random.Range(0, 16), 0};
                while (cycle[2] / 4 == cycle[0] / 4 || cycle[2] % 4 == cycle[0] % 4)
                    cycle[2] = Random.Range(0, 16);
                cycle[1] = (cycle[0] / 4) * 4 + (cycle[2] % 4);
                cycle[3] = (cycle[2] / 4) * 4 + (cycle[0] % 4);
                Debug.LogFormat("[Math 'em #{0}] Cycle tiles {1}{3} \u2192 {1}{4} \u2192 {2}{4} \u2192 {2}{3} \u2192 {1}{3}.", moduleID, "ABCD"[cycle[0] % 4], "ABCD"[cycle[2] % 4], (cycle[0] / 4) + 1, (cycle[2] / 4) + 1);
                for(int i = 0; i < 16; i++)
                {
                    if (cycle.Contains(i))
                    {
                        int n = (Array.IndexOf(cycle, i) + 1) % 4;
                        map[i] = cycle[n];
                    }
                    else
                        map[i] = i;
                }
                break;
        }
        return map.Select(k => x[k]).ToArray();
    }

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        tp = TwitchPlaysActive;
        module.OnActivate = Activate;
    }

    private void Activate()
    {
        tarrange[0] = Enumerable.Range(0, 16).ToArray();
        tarrange[5] = Enumerable.Range(0, 16).ToArray();
        int[] tassign = new int[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}.Shuffle();
        for(int i = 0; i < 16; i++)
        {
            tprops[i, 0] = tassign[i] < 4 ? 1 : (tassign[i] > 11 ? 2 : 0);
            tprops[i, 1] = Random.Range(0, 4);
            tprops[i, 2] = Random.Range(0, tprops[i, 0] == 2 ? 4 : 10);
            tlabels[i].material = tpatterns[(tprops[i, 0] * 10) + tprops[i, 2]];
            if (tprops[i, 1] > 0)
                tlabels[i].material.color = new Color32[3] { new Color32(200, 150, 50, 255), new Color32(175, 175, 175, 255), new Color32(230, 200, 50, 255)}[tprops[i, 1] - 1];
        }        
        matstore.SetActive(false);
        Interpret();
        foreach(KMSelectable button in keys)
        {
            int b = keys.IndexOf(button);
            switch (b)
            {
                case 10:
                    button.OnInteract = delegate () {
                        if (active[0])
                        {
                            button.AddInteractionPunch(0.4f);
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            sub /= 10;
                            disp.text = sub.ToString();
                        } return false; }; break;
                case 11:
                    button.OnInteract = delegate ()
                    {
                        if (active[0])
                        {
                            Debug.LogFormat("[Math 'em #{0}] Submitted {1}.", moduleID, sub);
                            button.AddInteractionPunch(0.4f);
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            if (ans == sub)
                            {
                                active[0] = false;
                                Audio.PlaySoundAtTransform("InputCorrect", transform);
                                if (phasetwo)
                                {
                                    module.HandlePass();
                                    if (!tp)
                                        StartCoroutine(Flip());
                                    disp.text = "CORRECT";
                                }
                                else
                                {
                                    phasetwo = true;
                                    disp.text = string.Empty;
                                    active[1] = true;
                                    StartCoroutine(Open(true));
                                }
                            }
                            else
                            {
                                module.HandleStrike();
                                if (phasetwo)
                                {
                                    active[0] = false;
                                    StartCoroutine(R());
                                }
                                else
                                {
                                    disp.text = "0";
                                }
                            }
                        }
                        sub = 0;
                        return false;
                    }; break;
                case 12:
                    button.OnInteract = delegate () {
                        if (active[1])
                        {
                            active[1] = false;
                            button.AddInteractionPunch();
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Stamp, button.transform);
                            StartCoroutine(Rearrange());
                        }
                        return false;
                    }; break;
                default:
                    button.OnInteract = delegate () {
                        if (active[0] && sub < 10000000)
                        {
                            button.AddInteractionPunch(0.2f);
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            sub *= 10;
                            sub += b;
                            disp.text = sub.ToString();
                        }
                        return false;
                    }; break;
            }
        }
    }

    private IEnumerator Flip()
    {
        for (int i = 0; i < 16; i++)
            tiles[i].transform.localPosition += new Vector3(0, 0.006f, 0);
        for (int j = 0; j < 30; j++)
        {
            for (int i = 0; i < 16; i++)
                tiles[i].transform.RotateAround(tiles[i].transform.position, transform.forward, 6);
            yield return new WaitForSeconds(1 / 30f);
        }
        for (int i = 0; i < 16; i++)
            tiles[i].transform.localPosition -= new Vector3(0, 0.006f, 0);
    }

    private IEnumerator Move(int tile, int end)
    {
        float[][] coords = new float[2][] { new float[2] { tiles[tile].transform.localPosition.x, tiles[tile].transform.localPosition.z }, new float[2] { new float[] { -0.03354406f, -0.01130594f, 0.01102563f, 0.03317031f }[end % 4], new float[] { 0.04113839f, 0.01354384f, -0.0141568f, -0.04175134f }[end / 4] } };
        for (int i = 0; i < 40; i++)
        {
            yield return new WaitForSeconds(1/30f);
            tiles[tile].transform.localPosition = new Vector3(Mathf.Lerp(coords[0][0], coords[1][0], (float)i / 39), 0.0023f, Mathf.Lerp(coords[0][1], coords[1][1], (float)i / 39));
        }
        tiles[tile].transform.localPosition -= new Vector3(0, 0.001f, 0);
    }

    private IEnumerator Open(bool t)
    {
        for (int j = 0; j < 20; j++)
        {
            Transform c = doorpivot.transform;
            if (t)
            {
                doors[0].transform.RotateAround(doorpivot.transform.position, c.up, 2);
                doors[1].transform.RotateAround(doorpivot.transform.position, c.up, -2);
            }
            else
            {
                doors[0].transform.RotateAround(doorpivot.transform.position, c.up, -2);
                doors[1].transform.RotateAround(doorpivot.transform.position, c.up, 2);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator Rearrange()
    {
        Debug.LogFormat("[Math 'em #{0}] Shuffle initiated:", moduleID);
        StartCoroutine(Flip());
        StartCoroutine(Open(false));
        Audio.PlaySoundAtTransform("Activate", transform);
        yield return new WaitForSeconds(3);
        disp.text = "5";
        for(int i = 0; i < 5; i++)
        {
            Audio.PlaySoundAtTransform("Shuffle", transform);
            tarrange[i + 1] = Swap(tarrange[i]);
            foreach(int c in tarrange[i].Where((x, j) => tarrange[i + 1][j] != x))
                    StartCoroutine(Move(c, Array.IndexOf(tarrange[i + 1], c)));
            yield return new WaitForSeconds(2.1f);
            disp.text = (4 - i).ToString();
        }        
        Interpret();
        active[0] = true;
    }

    private IEnumerator R()
    {
        disp.text = "";
        Debug.LogFormat("[Math 'em #{0}] Arrangement reset.", moduleID);
        StartCoroutine(Flip());
        yield return new WaitForSeconds(3);
        Audio.PlaySoundAtTransform("Reset", transform);
        for (int i = 0; i < 16; i++)
            StartCoroutine(Move(i, i));
        yield return new WaitForSeconds(2);
        active[1] = true;
        StartCoroutine(Open(true));
    }

    bool TwitchPlaysActive;

    private string TwitchHelpMessage = "!{0} enter <0-9> [Inputs digits] | !{0} back [Deletes last input] | !{0} clear [Deletes all inputs] | !{0} submit | !{0} activate [Begins shuffling]";

    private IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        if(command == "activate")
        {
            if (active[1])
                keys[12].OnInteract();
            else if (phasetwo)
                yield return "sendtochaterror!f The tiles are already shuffled.";
            else
                yield return "sendtochaterror!f The tiles may not be shuffled at this stage.";
            yield break;
        }
        if (!active[0])
        {
            yield return "sendtochaterror!f The keypad may not be interacted with while the tiles are shuffling.";
            yield break;
        }
        if(command == "back")
        {
            keys[10].OnInteract();
            yield break;
        }
        if(command == "clear")
        {
            while(sub > 0)
            {
                yield return null;
                keys[10].OnInteract();
            }
            yield break;
        }
        if(command == "submit")
        {
            keys[11].OnInteract();
            yield break;
        }
        else
        {
            string[] commands = command.Split(' ');
            commands = new string[2] { commands[0], commands.Where((x, i) => i > 0).Join()};
            if(commands[0] != "enter")
            {
                yield return "sendtochaterror!f Invalid entry command";
                yield break;
            }
            if(commands[1].Any(x => !"0123456789".Contains(x)))
            {
                yield return "sendtochaterror!f Cannot enter NaN.";
                yield break;
            }
            if(commands[1].Length + (sub == 0 ? 0 : disp.text.Length) > 8)
            {
                yield return "sendtochaterror!f Entry exceeds maximum value.";
                yield break;
            }
            for(int i = 0; i < commands[1].Length; i++)
            {
                yield return null;
                int k = int.Parse(commands[1][i].ToString());
                keys[k].OnInteract();
            }
        }
    }

    // Autosolver by Kilo Bites

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;

        while (!active[0])
        {
            yield return true;
        }

        if (disp.text != "0")
        {
            while (!ans.ToString().StartsWith(disp.text))
            {
                if (disp.text == "0")
                {
                    break;
                }
                keys[10].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (!phasetwo && !active[1])
        {

            var answerKeyA = ans.ToString().Select(x => "0123456789".IndexOf(x)).ToArray();
            var startingIx = disp.text == "0" ? 0 : disp.text.Length;

            for (int i = startingIx; i < answerKeyA.Length; i++)
            {
                keys[answerKeyA[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            keys[11].OnInteract();
            yield return new WaitForSeconds(0.1f);

            while (!active[1])
            {
                yield return true;
            }
        }

        if (active[1])
        {
            keys[12].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while (!active[0])
        {
            yield return true;
        }
        if (phasetwo)
        {
            var answerKeyB = ans.ToString().Select(x => "0123456789".IndexOf(x)).ToArray();
            var startingIx = disp.text == "0" ? 0 : disp.text.Length;

            for (int i = startingIx; i < answerKeyB.Length; i++)
            {
                keys[answerKeyB[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            keys[11].OnInteract();
            yield return new WaitForSeconds(0.1f);
            
        }
    }
}
