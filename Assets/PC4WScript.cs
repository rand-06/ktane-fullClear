using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using CustomExtensions;
using KeepCoding;

public class PC4WScript : MonoBehaviour
{
    private float DAS, ARR, SDF, L1, L2, L3;
    private KeyCode holdKey, ccwKey, cwKey, updKey, hardDropKey, leftKey, rightKey, downKey;
    private readonly float baseGravity = 0.800f, gravityIncrement = 0.007f; // baseGravity is incremented by gravityIncrement each second
    float onGroundCurrent, onGroundMove, onGroundTotal;
    private bool TwitchPlaysActive;

    public TextMesh piecesText, velocityText, fullClearsText, linesText, comboText;
    private int piecesPlaced, fullClears, linesCleared, currentCombo;
    private float totalTime;

    private readonly string pieces = "ZLOSIJT";

    public AudioClip dropSound, fillSound;
    public List<AudioClip> comboSounds;

    
    private readonly List<List<Vector2Int>> 
    regularRotTable = new List<List<Vector2Int>> {
        new List<Vector2Int>{new Vector2Int( 0, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0, 0)},
        new List<Vector2Int>{new Vector2Int( 0, 0),	new Vector2Int(+1, 0),	new Vector2Int(+1,-1),	new Vector2Int( 0,+2),	new Vector2Int(+1,+2)},
        new List<Vector2Int>{new Vector2Int( 0, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0, 0)},
        new List<Vector2Int>{new Vector2Int( 0, 0),	new Vector2Int(-1, 0),	new Vector2Int(-1,-1),	new Vector2Int( 0,+2),	new Vector2Int(-1,+2)}
    }, IRotTable = new List<List<Vector2Int>>{
        new List<Vector2Int>{new Vector2Int( 0, 0),	new Vector2Int(-1, 0),	new Vector2Int(+2, 0),	new Vector2Int(-1, 0),	new Vector2Int(+2, 0)},
        new List<Vector2Int>{new Vector2Int(-1, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0, 0),	new Vector2Int( 0,+1),	new Vector2Int( 0,-2)},
        new List<Vector2Int>{new Vector2Int(-1,+1),	new Vector2Int(+1,+1),	new Vector2Int(-2,+1),	new Vector2Int(+1, 0),	new Vector2Int(-2, 0)},
        new List<Vector2Int>{new Vector2Int( 0,+1),	new Vector2Int( 0,+1),	new Vector2Int( 0,+1),	new Vector2Int( 0,-1),	new Vector2Int( 0,+2)}
    }, ORotTable = new List<List<Vector2Int>> {
        new List<Vector2Int>{new Vector2Int( 0, 0)},
        new List<Vector2Int>{new Vector2Int( 0,-1)},
        new List<Vector2Int>{new Vector2Int(-1,-1)},
        new List<Vector2Int>{new Vector2Int(-1, 0)}
    };
        

    private readonly Dictionary<char,List<Vector2Int>> shapeInfo = new Dictionary<char,List<Vector2Int>> {
        { 'Z', new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1) }},
        { 'L', new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) }},
        { 'O', new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }},
        { 'S', new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }},
        { 'I', new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) }},
        { 'J', new List<Vector2Int> { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) }},
        { 'T', new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) }},
    };
    private readonly Dictionary<char, Color> colorDict = new Dictionary<char, Color> {
        { 'Z', new Color(0xd7 / 255f, 0x0f / 255f, 0x37 / 255f) },
        { 'L', new Color(0xe3 / 255f, 0x5b / 255f, 0x02 / 255f) },
        { 'O', new Color(0xe3 / 255f, 0x9f / 255f, 0x02 / 255f) },
        { 'S', new Color(0x59 / 255f, 0xb1 / 255f, 0x01 / 255f) },
        { 'I', new Color(0x0f / 255f, 0x9b / 255f, 0xd7 / 255f) },
        { 'J', new Color(0x21 / 255f, 0x41 / 255f, 0xc6 / 255f) },
        { 'T', new Color(0xaf / 255f, 0x29 / 255f, 0x8a / 255f) },

        { 'z', new Color(0xd7 / 255f * 0.5f, 0x0f / 255f * 0.5f, 0x37 / 255f * 0.5f) },
        { 'l', new Color(0xe3 / 255f * 0.5f, 0x5b / 255f * 0.5f, 0x02 / 255f * 0.5f) },
        { 'o', new Color(0xe3 / 255f * 0.5f, 0x9f / 255f * 0.5f, 0x02 / 255f * 0.5f) },
        { 's', new Color(0x59 / 255f * 0.5f, 0xb1 / 255f * 0.5f, 0x01 / 255f * 0.5f) },
        { 'i', new Color(0x0f / 255f * 0.5f, 0x9b / 255f * 0.5f, 0xd7 / 255f * 0.5f) },
        { 'j', new Color(0x21 / 255f * 0.5f, 0x41 / 255f * 0.5f, 0xc6 / 255f * 0.5f) },
        { 't', new Color(0xaf / 255f * 0.5f, 0x29 / 255f * 0.5f, 0x8a / 255f * 0.5f) },
        
        { ' ', new Color(0, 0, 0) }
    };
    private bool held;
    public KMAudio Audio;
    public Transform gridParent;
    public GameObject square, queueSquare, holdSquare;
    private List<List<char>> cup;
    private Vector2Int spawnOrigin;

    private List<List<GameObject>> grid;
    private string queue = string.Empty;
    private char holdingPiece = '0';
    private int currentRotation; private char currentPiece = '0'; private Vector2Int currentPosition;
    private List<List<List<GameObject>>> queuePieces;
    private List<List<GameObject>> holdPiece;
    private bool gameIsGoing = true;
    private Coroutine moveCorout, gravCorout;
    private bool selected;

    void Start()
    {
        GetComponent<KMSelectable>().OnFocus += delegate { selected = true; };
        GetComponent<KMSelectable>().OnDefocus+= delegate { selected = false; };
        spawnOrigin = new Vector2Int(1,16);
        grid = Enumerable.Range(0, 15).Select(z => Enumerable.Range(0, 4).Select(x => {
            GameObject newSquare = x + z == 0 ? square : Instantiate(square, gridParent);
            newSquare.transform.localPosition += new Vector3(0.01f * x, 0f, 0.01f * z);
            return newSquare;
        }).ToList()).ToList();
        foreach (var row in grid) foreach (GameObject s in row) s.GetComponent<MeshRenderer>().material.color = Color.black;
        generateQueueGrid();
        clearQueue();
        clearHold();
    }

    void updateQueue()
    {
        for (int i = 0; i < queuePieces.Count; i++)
            for (int j = 0; j < queuePieces[i].Count; j++)
                foreach (GameObject s in queuePieces[i][j])
                    s.SetActive(j==pieces.IndexOf(queue[i]));
    }
    void updateHold()
    {
        for (int j = 0; j < holdPiece.Count; j++)
            foreach (GameObject s in holdPiece[j])
                s.SetActive(pieces[j]==holdingPiece);
    }
    
    void clearQueue()
    {
        foreach (var t in queuePieces)
            foreach (var t1 in t)
            foreach (GameObject s in t1)
                s.SetActive(false);
    }
    void clearHold()
    {
        foreach (var t in holdPiece)
        foreach (GameObject s in t)
            s.SetActive(false);
    }
    
    void generateQueueGrid()
    {
        List<Vector2> offsets = new List<Vector2>
        {
            new Vector2(0f, 0.5f), new Vector2(0, 0.5f), new Vector2(.5f, .5f), new Vector2(0f, .5f),
            new Vector2(0.5f, 0f), new Vector2(0, 0.5f), new Vector2(0, 0.5f)
        };
        Vector2 pieceOffset = new Vector2(0f, -2.5f);
        queuePieces = 
            Enumerable.Range(0, 5).Select(q => 
                Enumerable.Range(0, 7).Select(p => 
                    Enumerable.Range(0, 4).Select(s => {
            GameObject newSquare = Instantiate(queueSquare, gridParent);
            Vector2 fullOffset = (s == 0 ? Vector2.zero : shapeInfo[pieces[p]][s - 1]) - offsets[p] + q*pieceOffset;
            newSquare.transform.localPosition += new Vector3(fullOffset.x, 0f, fullOffset.y) * 0.01f;
            newSquare.GetComponent<MeshRenderer>().material.color = colorDict[pieces[p]];
            return newSquare;
        }).ToList()).ToList()).ToList();
        queueSquare.SetActive(false);
        holdPiece = Enumerable.Range(0, 7).Select(p =>
            Enumerable.Range(0, 4).Select(s => {
                GameObject newSquare = Instantiate(holdSquare, gridParent);
                Vector2 fullOffset = (s == 0 ? Vector2.zero : shapeInfo[pieces[p]][s - 1]) - offsets[p];
                newSquare.transform.localPosition += new Vector3(fullOffset.x, 0f, fullOffset.y) * 0.01f;
                newSquare.GetComponent<MeshRenderer>().material.color = colorDict[pieces[p]];
                return newSquare;
            }).ToList()).ToList();
        holdSquare.SetActive(false);
    }
    string generateQueue() => queue.Length == 0 ? "TZS".shuffle() + "JOLI".shuffle() : pieces.shuffle();

    void spawn()
    {
        if (checkIfFits(queue[0], spawnOrigin))
        {
            currentPiece = queue[0];
            currentRotation = 0;
            currentPosition = spawnOrigin;
            queue = queue.Substring(1);
            updateQueue();
            if (queue.Length < 25) queue += generateQueue();
            drawCup();

        }
        else
        {
            gameIsGoing = false;
            Audio.PlaySoundAtTransform(fillSound.name, transform);
            clearQueue();
        }
    }

    Vector2Int getGhost(Vector2Int p, int r, char piece)
    {
        int y = p.y;
        while (checkIfFits(piece, new Vector2Int(p.x, y), r)) y--;
        return new Vector2Int(p.x, y+1);
    }
    
    void drawCup()
    {
        List<List<char>> modifiedCup = cup
            .DeepCopy()
            .addPiece(currentPiece.ToString().ToLower()[0], getGhost(currentPosition, currentRotation, currentPiece),
                currentRotation, shapeInfo[currentPiece])
            .addPiece(currentPiece, currentPosition, currentRotation, shapeInfo[currentPiece]);
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                grid[i][j].GetComponent<MeshRenderer>().material.color = colorDict[modifiedCup[i][j]];
            }
        }
    }

    bool checkIfFits(char piece, Vector2Int origin, int rotation = 0)
    {
        return origin.x >= 0 && origin.x <= 3 && origin.y >= 0 && cup[origin.y][origin.x] == ' ' && shapeInfo[piece]
            .Select(x => x.rotate(rotation) + origin)
            .All(v => v.x > -1 && v.x < 4 && v.y > -1 && cup[v.y][v.x] == ' ');
    }

    void Hold()
    {
        if (held) return;
        held = true;
        if (holdingPiece != '0')
        {
            char temp = holdingPiece;
            holdingPiece = currentPiece;
            currentPiece = temp;
            currentRotation = 0;
            currentPosition = spawnOrigin;
            updateHold();
            drawCup();
        }
        else
        {
            holdingPiece = currentPiece;
            updateHold();
            updateQueue();
            spawn();
        }
    }

    void Rotate(int rotation)
    {
        var table = currentPiece == 'O' ? ORotTable : currentPiece == 'I' ? IRotTable : regularRotTable;
        for (int i = 0; i < table[0].Count; i++)
        {
            Vector2Int offset = table[(currentRotation+rotation)%4][i] - table[currentRotation][i];
            if (checkIfFits(currentPiece, currentPosition + offset, currentRotation + rotation))
            {
                currentRotation = (currentRotation + rotation) % 4;
                currentPosition += offset;
                onGroundCurrent = 0f;
                onGroundMove = 0f;
                drawCup();
                return;
            }
            print("Layer " + i + " failed to rotate.");
        }

    }

    void newGame()
    {
        gameIsGoing = false;
        if (moveCorout!=null) StopCoroutine(moveCorout);
        if (gravCorout!=null) StopCoroutine(gravCorout);
        cup = Enumerable.Range(0, 25).Select(_ => Enumerable.Range(0, 4).Select(__ => ' ').ToList()).ToList();
        queue = "";
        held = false;
        for (int _ = 0; _ < 4; _++) queue += generateQueue();
        holdingPiece = '0';
        currentPiece = '0';
        currentRotation = 0;
        onGroundCurrent = 0f;
        onGroundMove = 0f;
        onGroundTotal = 0f;
        gameIsGoing = true;
        updateQueue();
        updateHold();
        spawn();
        totalTime = 0f;
        piecesPlaced = 0;
        fullClears = 0;
        linesCleared = 0;
        currentCombo = 0;
        comboText.text = "";
        piecesText.text = "0";
        velocityText.text = "(0.00/s)";
        fullClearsText.text = "0";
        linesText.text = "0";
        moveCorout = StartCoroutine(Move());
        gravCorout = StartCoroutine(gravity());
        
    }

    void drop()
    {
        held = false;
        bool holdCombo = false;
        onGroundCurrent = 0f;
        onGroundMove = 0f;
        onGroundTotal = 0f;
        cup = cup.addPiece(currentPiece, getGhost(currentPosition, currentRotation, currentPiece), currentRotation, shapeInfo[currentPiece]);
        piecesPlaced++;
        piecesText.text = piecesPlaced.ToString();
        velocityText.text = "(" + (piecesPlaced / (totalTime + 0.001f)).ToString("F2") + "/s)";
        for (int i = 20; i < 25; i++)
        {
            for(int j=0;j<4;j++) cup[i][j] = ' ';
        }
        for (int i = 0; i < 20; i++)
        {
            if (cup[i].All(x => x != ' '))
            {
                holdCombo = true;
                linesCleared++;
                linesText.text = linesCleared.ToString();
                for(int j=0;j<4;j++) cup[i][j] = ' ';
                for (int j=i; j<20; j++) for (int k=0; k<4;k++) cup[j][k] = cup[j+1][k];
                i--;
            }
        }
        if (holdCombo)
        {
            currentCombo++;
            Audio.PlaySoundAtTransform(comboSounds[
                (currentCombo >= comboSounds.Count ? comboSounds.Count : currentCombo)-1].name, transform);
        }
        else
        {
            Audio.PlaySoundAtTransform(dropSound.name, transform);
            currentCombo = 0;
        }
        comboText.text = currentCombo==0?"":"COMBO x" + currentCombo;
        if (cup[0].All(x => x == ' '))
        {
            fullClears++;
            fullClearsText.text = fullClears.ToString();
            GetComponent<KMBombModule>().HandlePass();
        }
        spawn();
    }

    IEnumerator Move()
    {
        while (gameIsGoing)
        {
            totalTime += Time.deltaTime;
            velocityText.text = "(" + (piecesPlaced / (totalTime + 0.001f)).ToString("F2") + "/s)";
            if (direction != 0)
            {
                float timer = 0f, threshold = 0f;
                while (direction != 0)
                {
                    timer += Time.deltaTime;
                    if (timer >= threshold)
                    {
                        threshold += threshold == 0 ? DAS : ARR;
                        if (checkIfFits(currentPiece, currentPosition + new Vector2Int(direction, 0), currentRotation))
                        {
                            currentPosition += new Vector2Int(direction, 0);
                            onGroundCurrent = 0f;
                            drawCup();
                        }
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            else yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator gravity()
    {
        float timer = 0f, timer2 = 0f, currentGravity = baseGravity;
        
        while (gameIsGoing)
        {
            timer += Time.deltaTime;
            timer2 += Time.deltaTime;
            if (timer >= 1f / currentGravity / (holdingDown ? SDF : 1f) / 2.5f)
            {
                if (checkIfFits(currentPiece, currentPosition + Vector2Int.down, currentRotation))
                {
                    timer = 0f;
                    currentPosition += Vector2Int.down;
                    drawCup();
                }
                else
                {
                    onGroundCurrent += Time.deltaTime;
                    onGroundMove += Time.deltaTime;
                    onGroundTotal += Time.deltaTime;
                    if (onGroundCurrent >= L1 || onGroundMove >= L2 || onGroundTotal >= L3){
                        drop();
                    }
                    yield return new WaitForEndOfFrame();
                }
            }

            if (timer2 >= 1f)
            {
                currentGravity += gravityIncrement;
                timer2 = 0f;
            }
            yield return new WaitForEndOfFrame();
        }
    }
    
    private int direction;
    private bool holdingDown;
    void Update()
    {
        if (!selected) return;

        if (Input.GetKeyDown(KeyCode.F4)) newGame();
        if (Input.GetKeyDown(holdKey)) Hold();
        if (Input.GetKeyDown(ccwKey)) Rotate(3);
        if (Input.GetKeyDown(updKey)) Rotate(2);
        if (Input.GetKeyDown(cwKey)) Rotate(1);
        if (Input.GetKeyDown(hardDropKey)) drop();
        if (Input.GetKey(leftKey) == Input.GetKey(rightKey)) direction = 0;
        else if (Input.GetKey(rightKey)) direction = 1;
        else if (Input.GetKey(leftKey)) direction = -1;
        if (Input.GetKeyDown(downKey)) holdingDown = true;
        if (Input.GetKeyUp(downKey)) holdingDown = false;
    }
    
    
    private FullClearSettings Settings = new FullClearSettings();
    
    
	
    class FullClearSettings
    {
        public float 
            DAS = 0.167f,
            ARR = 0.033f,
            SDF = 6.000f,
            L1 = 0.500f,
            L2 = 5.000f,
            L3 = 20.000f;
        public string 
            holdBind = "LSHIFT", 
            ccwBind = "X",
            updBind = "A", 
            cwBind = "Z", 
            hardDropBind = "SPACE", 
            leftBind = "LEFT", 
            rightBind = "RIGHT", 
            downBind = "DOWN";
    }

    private void Awake()
    {
        ModConfig<FullClearSettings> modConfig = new ModConfig<FullClearSettings>("FullClearSettings");
        Settings = modConfig.Settings;
        modConfig.Settings = Settings;
        DAS = Settings.DAS;
        ARR = Settings.ARR;
        SDF = Settings.SDF;
        L1 = Settings.L1;
        L2 = Settings.L2;
        L3 = Settings.L3;
        holdKey = Settings.holdBind.stringToKeyCode();
        ccwKey = Settings.ccwBind.stringToKeyCode();
        updKey = Settings.updBind.stringToKeyCode();
        cwKey = Settings.cwBind.stringToKeyCode();
        hardDropKey = Settings.hardDropBind.stringToKeyCode();
        leftKey = Settings.leftBind.stringToKeyCode();
        rightKey = Settings.rightBind.stringToKeyCode();
        downKey = Settings.downBind.stringToKeyCode();
        comboText.text = "";
        piecesText.text = "";
        velocityText.text = "";
        fullClearsText.text = "";
        linesText.text = "";
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "FullClearSettings.json" },
            { "Name", "Full Clear! Settings" },
            { "Listings", new List<Dictionary<string, object>>{
                new Dictionary<string, object>
                {
                    { "Key", "DAS" },
                    { "Text", "DAS" },
                    { "Description", "Default is 0.167 seconds." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "ARR" },
                    { "Text", "ARR" },
                    { "Description", "Default is 0.033 seconds." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "SDF" },
                    { "Text", "SDF" },
                    { "Description", "Default is 6.000x." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "L1" },
                    { "Text", "L1" },
                    { "Description", "Time for inactivity when soft-dropped, default is 0.500 seconds." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "L2" },
                    { "Text", "L2" },
                    { "Description", "Same as L1, but you can move, default is 5.000 seconds." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "L3" },
                    { "Text", "L3" },
                    { "Description", "Forced hard-drop after soft-drop, default is 20.000 seconds." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "holdBind" },
                    { "Text", "Hold" },
                    { "Description", "Default is LSHIFT." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "ccwBind" },
                    { "Text", "Rotate CCW" },
                    { "Description", "Default is X." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "cwBind" },
                    { "Text", "Rotate CW" },
                    { "Description", "Default is Z." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "updBind" },
                    { "Text", "Rotate upside-down" },
                    { "Description", "Default is A." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "hardDropBind" },
                    { "Text", "Hard-drop" },
                    { "Description", "Default is SPACE." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "downBind" },
                    { "Text", "Soft-drop" },
                    { "Description", "Default is DOWN." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "leftBind" },
                    { "Text", "Left" },
                    { "Description", "Default is LEFT." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "rightBind" },
                    { "Text", "Right" },
                    { "Description", "Default is RIGHT." }
                }
            } }
        }
    };
    
    
}

namespace CustomExtensions { public static class Extensions
    {
        private static Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>
        {
            { "LSHIFT", KeyCode.LeftShift },
            { "RSHIFT", KeyCode.RightShift },
            { "LALT", KeyCode.LeftAlt },
            { "RALT", KeyCode.RightAlt },
            { "LCTRL", KeyCode.LeftControl },
            { "RCTRL", KeyCode.RightControl },
            { "TAB", KeyCode.Tab },
            { "UP", KeyCode.UpArrow },
            { "DOWN", KeyCode.DownArrow },
            { "LEFT", KeyCode.LeftArrow },
            { "RIGHT", KeyCode.RightArrow },
            { "SPACE", KeyCode.Space },
            { "ENTER", KeyCode.Return },
            { "RETURN", KeyCode.Return},
            { "`", KeyCode.BackQuote},
            { "[", KeyCode.LeftBracket},
            { "]", KeyCode.RightBracket},
            { ";", KeyCode.Semicolon},
            { "'", KeyCode.Quote},
            {",", KeyCode.Comma},
            { "-", KeyCode.Minus},
            { "=", KeyCode.Equals},
            {".",KeyCode.Period},
            {"/", KeyCode.Slash},
            {"\\", KeyCode.Backslash}
        };

        public static KeyCode stringToKeyCode(this string s)
        {
            if (keys.ContainsKey(s.ToUpper())) return keys[s];
            if (s.Length == 1 && ((s[0] >= '0' && s[0] <= '9') || (s.ToLower()[0] >= 'a' && s.ToLower()[0] <= 'z')))
                return (KeyCode)s.ToLower()[0];
            return KeyCode.None;
        }
        
        public static string shuffle(this string s) => s.ToCharArray().Shuffle().Aggregate("", (a, b) => a + b);

        public static Vector2Int rotate(this Vector2Int vector, int rotation)
        {
            Vector2Int ans = vector;
            for (int i = 0; i < (rotation % 4 + 4) % 4; i++) ans = new Vector2Int(-ans.y, ans.x); // 1CW = +x -> -y -> -x -> +y -> +x
            return ans;
        }
        public static List<List<char>> addPiece(this List<List<char>> grid, char piece, Vector2Int position, int rotation, List<Vector2Int> offsets)
        {
            offsets.Add(Vector2Int.zero);
            foreach (Vector2Int offset in offsets)
            {
                Vector2Int final = position + offset.rotate(rotation);
                grid[final.y][final.x] = piece;
            }
            return grid;
        }

        public static List<List<T>> DeepCopy<T>(this List<List<T>> list)
        {
            List<List<T>> copy = Enumerable.Range(0, list.Count).Select(i =>
                    Enumerable.Range(0, list[i].Count).Select(j => list[i][j]).ToList()
                ).ToList();
            return copy;
        }
    }
}