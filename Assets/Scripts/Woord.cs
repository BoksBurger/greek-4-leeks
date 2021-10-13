using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Woorde
{
    public string Type;
    public string shortWords;
    public string fullWords;
    public string Alphabet;
    public string Meaning;
    public int Frequency;
}

[System.Serializable]
public class WoordData
{
    public Woorde[] data;
}

public class Woord : MonoBehaviour
{
    [SerializeField] Text griekseWoord;
    [SerializeField] Text engelseWoord;
    [SerializeField] Button skommelKnoppie;
    [SerializeField] Text showHideMeaningButtonText;
    [SerializeField] Slider frequencySlider;
    [SerializeField] Text frequencyText;
    [SerializeField] Text huidigeWoordFrekwensie;
    [SerializeField] Text woordTipe;

    [SerializeField] TextAsset woordData;
    List<int> temp = new List<int>();

    bool displayMeaning = true;

    WoordData woord;


    List<Woorde> woordeLys = new List<Woorde>();

    int woordTeller = 0;
    string woordGroep = "500+";

    bool isInteracting = false;
    float initialMouseY = 0f;
    float initialMouseX = 0f;
    /********************************/
    //Shake it up.
    float accelerometerUpdateInterval = 1.0f / 60.0f;
    // The greater the value of LowPassKernelWidthInSeconds, the slower the
    // filtered value will converge towards current input sample (and vice versa).
    float lowPassKernelWidthInSeconds = 1.0f;
    // This next parameter is initialized to 2.0 per Apple's recommendation,
    // or at least according to Brady! ;)
    float shakeDetectionThreshold = 2.0f;

    float lowPassFilterFactor;
    Vector3 lowPassValue;

    bool shakeStarted = false;
    bool waiting = false;

    /********************************/


    void Start()
    {
        //Shake start
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
        //Shake end

        skommelKnoppie.onClick.AddListener(VertoonNuweWoord);
        woord = JsonUtility.FromJson<WoordData>(woordData.text);
        SetFrequency();
    }

    private void VertoonNuweWoord()
    {
        if (temp.Count < woordeLys.Count)
        {
            int nuweWoordIndex = kryNuweWoordIndex();

            if (!temp.Contains(nuweWoordIndex))
            {
                temp.Add(nuweWoordIndex);
                woordTeller = temp.Count - 1;
                bindWordData(nuweWoordIndex);
                maintainFrequencyCount(nuweWoordIndex, "Exploring");
                shakeStarted = true;
                StopShakeAfter(1);
            }
            else
            {
                VertoonNuweWoord();
            }
        }
        else
        {
            Woorde[] w = woordeLys.ToArray();
            if (temp.Count == woordeLys.Count)
            {
                huidigeWoordFrekwensie.text = "";
                griekseWoord.text = "τέλος";
                engelseWoord.text = "end";
                frequencyText.text = "Congratulations!\nYou've explored all " + woordeLys.Count.ToString() + " words\nin the frequency group: " + woordGroep;
            }

        }
    }

    private void bindWordData(int woordIndex)
    {
        Woorde[] w = woordeLys.ToArray();
        griekseWoord.text = w[woordIndex].shortWords;
        engelseWoord.text = w[woordIndex].Meaning;
        woordTipe.text = "(" + w[woordIndex].Type + ")";
    }

    private void maintainFrequencyCount(int woordIndex, string reviewText)
    {
        Woorde[] w = woordeLys.ToArray();
        string frequency = w[woordIndex].Frequency.ToString() + " times.";
        if (frequency == "2 times.")
        {
            frequency = "twice.";
        }
        else if (frequency == "1 times.")
        {
            frequency = "once.";
        }
        huidigeWoordFrekwensie.text = "Occurring " + frequency;
        frequencyText.text = reviewText + " " + (woordTeller + 1).ToString() + " of " + woordeLys.Count.ToString();

    }

    private int kryNuweWoordIndex()
    {
        Woorde[] w = woordeLys.ToArray();
        int aantalWoorde = w.Length;
        int nuweWoordIndex = Random.Range(0, aantalWoorde);
        return nuweWoordIndex;
    }

    void Update()
    {
        InterActions();
    }

    private void InterActions()
    {
        //Monitor drag & swipes
        if (Input.GetMouseButtonDown(0))
        {
            initialMouseX = Input.mousePosition.x;
            initialMouseY = Input.mousePosition.y;

        }
        if (Input.GetMouseButtonUp(0))
        {
            //Quit on drag down
            float upY = Input.mousePosition.y;
            if ((initialMouseY - upY) > (Screen.height / 2))
            {
                Quit();
            }
            //Navigate on swipe
            if (temp.Count > 0)
            {
                Vector3 V3 = Input.mousePosition;
                if ((initialMouseX - V3.x) > (Screen.width / 2))
                {
                    VolgendeWoord();

                }
                else if ((initialMouseX - V3.x) < (Screen.width / 2 * (-1)))
                {
                    VorigeWoord();
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        //Allows for screnshots
        if (Input.GetKeyDown(KeyCode.P))
        {
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/" + griekseWoord.text + ".png");
        }
        //Shake it up
        Vector3 acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        Vector3 deltaAcceleration = acceleration - lowPassValue;
        if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
        {
            if (!shakeStarted)
            {
                VertoonNuweWoord();
            }
        }
        if (shakeStarted && !waiting)
        {
            StartCoroutine(StopShakeAfter(1f));
        }
    }

    System.Collections.IEnumerator StopShakeAfter(float fSeconds)
    {
        //Debug.Log("Started wating for " + fSeconds.ToString());
        waiting = true;
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(fSeconds);
        yield return wait;
        shakeStarted = false;
        waiting = false;
        //Debug.Log("Waited long enough.");
    }

    public void VorigeWoord()
    {
        if (woordTeller > 0)
        {
            if (engelseWoord.text != "end")
            {
                woordTeller--;
            }
            int vorigeIndex = (temp.ToArray())[woordTeller];
            bindWordData(vorigeIndex);
            maintainFrequencyCount(woordTeller, "Showing");
        }
        //Debug.Log(woordTeller);
    }

    public void VolgendeWoord()
    {
        if (woordTeller < temp.Count - 1)
        {
            woordTeller++;
            int volgendeIndex = (temp.ToArray())[woordTeller];
            bindWordData(volgendeIndex);
            maintainFrequencyCount(woordTeller, "Showing");
        }
        else
        {
            VertoonNuweWoord();
        }
        //Debug.Log(woordTeller);

    }

    public void SetFrequency()
    {
        int min = 0;
        int max = 0;
        switch (frequencySlider.value)
        {
            case 1:
                woordGroep = "500+";
                min = 500;
                max = 1000000;
                break;
            case 2:
                woordGroep = "250-500";
                min = 250;
                max = 500;
                break;
            case 3:
                woordGroep = "100-250";
                min = 100;
                max = 250;
                break;
            case 4:
                woordGroep = "50-100";
                min = 50;
                max = 100;
                break;
            case 5:
                min = 25;
                max = 50;
                woordGroep = "25-50";
                break;
            case 6:
                woordGroep = "20-25";
                min = 20;
                max = 25;
                break;
            case 7:
                woordGroep = "15-20";
                min = 15;
                max = 20;
                break;
            case 8:
                woordGroep = "10-15";
                min = 10;
                max = 15;
                break;
            case 9:
                woordGroep = "9";
                min = 9;
                max = 9;
                break;
            case 10:
                woordGroep = "8";
                min = 8;
                max = 8;
                break;
            case 11:
                woordGroep = "7";
                min = 7;
                max = 7;
                break;
            case 12:
                woordGroep = "6";
                min = 6;
                max = 6;
                break;
            case 13:
                woordGroep = "5";
                min = 5;
                max = 5;
                break;
            case 14:
                woordGroep = "4";
                min = 4;
                max = 4;
                break;
            case 15:
                woordGroep = "3";
                min = 3;
                max = 3;
                break;
            case 16:
                woordGroep = "2";
                min = 2;
                max = 2;
                break;
            case 17:
                woordGroep = "1";
                min = 1;
                max = 1;
                break;
            default:
                woordGroep = "500+";
                break;
        }
        woordeLys.Clear();
        temp.Clear();
        foreach (Woorde w in woord.data)
        {
            if (w.Frequency >= min && w.Frequency <= max)
            {
                woordeLys.Add(w);
            }
        }
        frequencyText.text = "Frequency: " + woordGroep + "\n\nWord count: " + woordeLys.Count;
        if (woordGroep != "500+")
        {
            griekseWoord.text = woordGroep;
            engelseWoord.text = "Start exploring\n" + woordeLys.Count + " words.";
            woordTipe.text = "";
            huidigeWoordFrekwensie.text = "";
        }
        else
        {
            griekseWoord.text = "ὑποδέχομαι";
            engelseWoord.text = "to welcome, receive, entertain as a guest";
            woordTipe.text = "";
            huidigeWoordFrekwensie.text = "";
        }
    }
    public void showHideMeaning()
    {
        string[] hidden = { "ἀποκρύπτω", "χρηματίζω" }; //Conceal - Reveal answer
        string sShowHide;
        displayMeaning = !displayMeaning;
        if (displayMeaning)
            sShowHide = hidden[0];
        else
            sShowHide = hidden[1];
        showHideMeaningButtonText.text = sShowHide;
        engelseWoord.gameObject.SetActive(displayMeaning);
    }
    public void Quit()
    {
        Application.Quit();
    }


}
