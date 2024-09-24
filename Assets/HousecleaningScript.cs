using System.Collections;
using UnityEngine;
using System;

public class HousecleaningScript : MonoBehaviour {

    public KMAudio audio;
    public KMSelectable[] buttons;
    public GameObject[] grid;

    private string[] houseLayout = {
        "LT2", "T", "TR1", "LT", "T", "T", "T4", "T", "T", "T2", "T", "TR3",
        "L", "", "R", "L", "3", "", "", "", "", "", "", "R",
        "L", "1", "R", "L", "2", "", "", "1", "", "", "", "",
        "L", "", "R", "L", "", "", "", "2", "", "4", "1", "R",
        "LB2", "", "RB3", "LB", "", "B3", "B", "B", "B", "", "B", "RB",
        "LT", "", "T", "T4", "", "T", "T", "TR", "LT", "", "T", "TR",
        "L", "3", "", "1", "", "", "", "1", "", "3", "", "R",
        "LB1", "B", "2", "B", "B", "B4", "", "RB", "L", "", "", "R1",
        "LT", "T", "", "T4", "TR2", "LT", "", "TR", "L", "", "4", "R2",
        "L", "1", "", "3", "R", "L", "2", "R", "L", "", "", "R",
        "L", "", "", "", "R", "L", "", "R1", "L4", "", "3", "R",
        "L", "4", "", "", "R", "L", "3", "R", "L", "", "", "R4",
        "LB", "", "B2", "B", "BR3", "LB", "B4", "BR", "LB", "B", "B", "BR"
    };
    private string[] trash = { "Red Shell", "Banana", "Green Shell", "Fake Item Box" };
    private string[] cleaners = { "Doxi Kleen", "Tiffer Sweeper", "Simonly Steamer", "Shamrocks" };
    private int[] chosableCenters = new int[72];
    private int[] itemMap = { 4, 1, 3, 2 };
    private HouseCell[] chosenCells = new HouseCell[25];
    private int selectedCleaner = -1;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        foreach (GameObject obj in grid)
        {
            KMSelectable pressed = obj.GetComponent<KMSelectable>();
            pressed.OnInteract += delegate () { PressGrid(pressed); return false; };
        }
    }

    void Start()
    {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 8; j++)
                chosableCenters[8 * i + j] = (i * 12) + 26 + j;
        int choice = chosableCenters[UnityEngine.Random.Range(0, chosableCenters.Length)];
        int[] offsets = { -26, -25, -24, -23, -22, -14, -13, -12, -11, -10, -2, -1, 0, 1, 2, 10, 11, 12, 13, 14, 22, 23, 24, 25, 26 };
        Debug.LogFormat("[Housecleaning #{0}] The section on display is centered at {1}{2} of the house", moduleId, "ABCDEFGHIJKL"[choice % 12], choice / 12 + 1);
        for (int i = 0; i < 25; i++)
        {
            chosenCells[i] = new HouseCell(grid[i], houseLayout[offsets[i] + choice]);
            if (chosenCells[i].getItemIndex() > 0)
                Debug.LogFormat("[Housecleaning #{0}] There is a {1} at {2}{3} of the displayed section", moduleId, trash[chosenCells[i].getItemIndex() - 1], "ABCDE"[i % 5], i / 5 + 1);
        }
    }

    void PressGrid(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            int index = Array.IndexOf(grid, pressed.gameObject);
            Debug.LogFormat("[Housecleaning #{0}] Selected the space at {1}{2}", moduleId, "ABCDE"[index % 5], index / 5 + 1);
            if (chosenCells[index].getIsPressed())
            {
                Debug.LogFormat("[Housecleaning #{0}] This space has already been cleaned! Strike!", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else if (chosenCells[index].getItemIndex() == 0)
            {
                Debug.LogFormat("[Housecleaning #{0}] This space has no trash! Strike!", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else if (selectedCleaner == -1)
            {
                Debug.LogFormat("[Housecleaning #{0}] You used... no product? I expected {1}! Strike!", moduleId, cleaners[chosenCells[index].getItemIndex() - 1]);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else if (chosenCells[index].getItemIndex() != itemMap[selectedCleaner])
            {
                Debug.LogFormat("[Housecleaning #{0}] You used the wrong product! I expected {1}! Strike!", moduleId, cleaners[chosenCells[index].getItemIndex() - 1]);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                audio.PlaySoundAtTransform("spray", pressed.transform);
                chosenCells[index].setIsPressed();
                if (CheckComplete())
                {
                    Debug.LogFormat("[Housecleaning #{0}] All spaces of the section have been cleaned, module solved!", moduleId);
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                }
            }
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            pressed.AddInteractionPunch();
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            selectedCleaner = Array.IndexOf(buttons, pressed);
            Debug.LogFormat("[Housecleaning #{0}] Selected the product {1}", moduleId, cleaners[itemMap[selectedCleaner] - 1]);
        }
    }

    bool CheckComplete()
    {
        for (int i = 0; i < chosenCells.Length; i++)
            if (chosenCells[i].getItemIndex() > 0 && !chosenCells[i].getIsPressed())
                return false;
        return true;
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <H/D/S/T> [Selects the specified cleaning appliance] | !{0} <A-E><1-5> [Selects the space at the specified coordinate] | Commands are chainable with spaces";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.ToUpperInvariant().Split(' ');
        for (int i = 0; i < parameters.Length; i++)
            if (!parameters[i].EqualsAny("H", "D", "S", "T", "A1", "A2", "A3", "A4", "A5", "B1", "B2", "B3", "B4", "B5", "C1", "C2", "C3", "C4", "C5", "D1", "D2", "D3", "D4", "D5", "E1", "E2", "E3", "E4", "E5"))
                yield break;
        yield return null;
        for (int i = 0; i < parameters.Length; i++)
        {
            if ("HDST".Contains(parameters[i]))
                buttons["HDST".IndexOf(parameters[i])].OnInteract();
            else
                grid["ABCDE".IndexOf(parameters[i][0]) + 5 * "12345".IndexOf(parameters[i][1])].GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(.2f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (selectedCleaner != -1)
        {
            for (int i = 0; i < chosenCells.Length; i++)
            {
                if (chosenCells[i].getItemIndex() == itemMap[selectedCleaner] && !chosenCells[i].getIsPressed())
                {
                    grid[i].GetComponent<KMSelectable>().OnInteract();
                    yield return new WaitForSeconds(.2f);
                }
            }
        }
        if (!moduleSolved)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < chosenCells.Length; i++)
                {
                    if (chosenCells[i].getItemIndex() == itemMap[j] && !chosenCells[i].getIsPressed())
                    {
                        if (selectedCleaner != j)
                        {
                            buttons[j].OnInteract();
                            yield return new WaitForSeconds(.2f);
                        }
                        grid[i].GetComponent<KMSelectable>().OnInteract();
                        yield return new WaitForSeconds(.2f);
                    }
                }
            }
        }
    }
}