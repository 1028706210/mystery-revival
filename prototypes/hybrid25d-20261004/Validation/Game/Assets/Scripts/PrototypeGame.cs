using System;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryRevival
{
    // Deliberately small, code-built greybox. Rules remain in IncidentState.
    public sealed class PrototypeGame : MonoBehaviour
    {
        public IncidentState State { get; private set; }
        public int Floor { get; private set; } = 1;
        public Vector2 Position { get; private set; } = new Vector2(0, -4.1f);
        public string Panel { get; private set; } = "intro";
        public bool Parcel { get; private set; }
        public bool Accepted { get; private set; }
        public bool IsOpening => opening > 0;
        public bool Ready { get; private set; }
        readonly Dictionary<string, bool> doors = new Dictionary<string, bool>();
        readonly Dictionary<string, Vector2> marks = new Dictionary<string, Vector2>();
        readonly Dictionary<string, string> sourceNames = new Dictionary<string, string>();
        readonly List<GameObject> worldObjects = new List<GameObject>();
        readonly List<SpriteRenderer> doorRenderers = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> waterMarkRenderers = new List<SpriteRenderer>();
        Camera worldCamera;
        Transform person;
        Transform entryFrame;
        Sprite pixel;
        Sprite playerSprite;
        Sprite doorFrameSprite;
        Sprite doorLeafSprite;
        Sprite corridorWindowWallSprite;
        Font font;
        AudioSource audioSource;
        AudioClip knock, hum;
        GUIStyle label, small, heading, button, field;
        string toast = "雨还没停。楼里有人留了一盏灯。";
        float toastTime = 6, opening;
        int declaration = 1, actionNumber;
        string pendingDoor = "", pendingAction = "", dialogue = "";
        string dialogueKind = "";
        bool lamp = true, initialAnomaly;
        int knocksPlayed;
        double nextKnock;
        float knockPulse;
        Vector2 scroll;
        float scale;
        static readonly Color Ink = new Color(.055f,.074f,.08f);
        static readonly Color Paper = new Color(.83f,.83f,.76f);
        static readonly Color Muted = new Color(.46f,.58f,.56f);
        static readonly Color Accent = new Color(.78f,.58f,.34f);

        void Awake()
        {
            Application.targetFrameRate = 60;
            State = new IncidentState();
            var tex = new Texture2D(1,1) { filterMode = FilterMode.Point };
            tex.SetPixel(0,0,Color.white); tex.Apply();
            pixel = Sprite.Create(tex,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
            playerSprite = Resources.Load<Sprite>("nightwalker_idle_front");
            if(playerSprite==null) Debug.LogError("Missing Resources/nightwalker_idle_front Sprite; using player placeholder.");
            doorFrameSprite = Resources.Load<Sprite>("door_game_frame");
            doorLeafSprite = Resources.Load<Sprite>("door_game_leaf");
            corridorWindowWallSprite = Resources.Load<Sprite>("corridor_window_wall");
            font = Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},18);
            worldCamera = new GameObject("World camera").AddComponent<Camera>();
            worldCamera.transform.position = new Vector3(0,0,-10);
            worldCamera.orthographic = true; worldCamera.orthographicSize = 3.5f;
            worldCamera.backgroundColor = Ink;
            worldCamera.clearFlags = CameraClearFlags.SolidColor;
            worldCamera.rect = new Rect(0,0,1,1);
            audioSource = gameObject.AddComponent<AudioSource>();
            gameObject.AddComponent<AudioListener>();
            knock = Tone("Knock",.35f,95); hum = Tone("Room tone",2,53);
            audioSource.clip = hum; audioSource.loop = true; audioSource.volume=.12f; audioSource.Play();
            BuildWorld(); Ready=true;
            if (Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke")>=0)
                gameObject.AddComponent<PrototypeSmoke>();
        }

        AudioClip Tone(string title,float seconds,int hz)
        {
            int n=(int)(seconds*22050); var samples=new float[n];
            for(int i=0;i<n;i++) {
                double t=(double)i/22050;
                double envelope=title=="Knock" ? Math.Exp(-t*24) : .16;
                samples[i]=(float)(Math.Sin(t*hz*Math.PI*2)*envelope*.4);
            }
            var clip=AudioClip.Create(title,n,1,22050,false); clip.SetData(samples,0); return clip;
        }

        void Box(string name, Vector2 p, Vector2 size, Color color,int order=0)
        {
            var obj=new GameObject(name); obj.transform.position=p; obj.transform.localScale=size;
            var sr=obj.AddComponent<SpriteRenderer>(); sr.sprite=pixel; sr.color=color; sr.sortingOrder=order;
            worldObjects.Add(obj);
        }
        void BuildWorld()
        {
            doorRenderers.Clear();
            waterMarkRenderers.Clear();
            foreach(var obj in worldObjects) Destroy(obj); worldObjects.Clear();
            Box("Concrete",Vector2.zero,new Vector2(19,11),new Color(.19f,.23f,.23f),-2);
            for(int x=-9;x<=9;x++) for(int y=-5;y<=5;y++)
                Box("Tile",new Vector2(x,y),new Vector2(.96f,.96f),new Color(.23f+(x%2==0?.018f:0),.28f,.27f),-1);
            Box("North wall",new Vector2(0,5),new Vector2(19,.4f),new Color(.10f,.14f,.15f),1);
            Box("West wall",new Vector2(-9,0),new Vector2(.4f,10),Ink,1);
            Box("East wall",new Vector2(9,0),new Vector2(.4f,10),Ink,1);
            Box("Room west",new Vector2(-3,4),new Vector2(4.8f,1.5f),new Color(.12f,.17f,.18f));
            Box("Room east",new Vector2(3,4),new Vector2(4.8f,1.5f),new Color(.12f,.17f,.18f));
            for(int i=-8;i<=8;i++) if(Math.Abs(i)!=3&&Math.Abs(i)!=7)
                if(corridorWindowWallSprite==null||i!=0)
                    Box("Partition",new Vector2(i,2.9f),new Vector2(.95f,.3f),new Color(.12f,.16f,.17f),1);
            foreach(string side in new[]{"west","east"}) {
                bool open=DoorOpen(side); float x=side=="west"?-3:3;
                if(doorFrameSprite!=null&&doorLeafSprite!=null) BuildDoorVisual(side,new Vector2(x,2.45f),open);
                else Box("Door",new Vector2(x,2.9f),open?new Vector2(.18f,1.1f):new Vector2(.85f,.35f),
                    open?new Color(.35f,.32f,.23f):new Color(.46f,.35f,.22f),2);
                Box("Door mat",new Vector2(x,2.1f),new Vector2(1.15f,.42f),new Color(.29f,.25f,.21f),1);
            }
            for(int i=0;i<10;i++) {
                Box("Steps",new Vector2(-7,-.8f+i*.18f),new Vector2(1.5f,.07f),Muted,1);
                Box("Steps",new Vector2(7,-.8f+i*.18f),new Vector2(1.5f,.07f),Muted,1);
            }
            if(corridorWindowWallSprite!=null) {
                var windowWall=new GameObject("Corridor window wall"); windowWall.transform.position=new Vector2(0,2.45f);
                windowWall.transform.localScale=Vector3.one;
                var sr=windowWall.AddComponent<SpriteRenderer>(); sr.sprite=corridorWindowWallSprite; sr.sortingOrder=2;
                worldObjects.Add(windowWall);
            } else Box("Window",new Vector2(0,3),new Vector2(1.5f,.28f),new Color(.4f,.63f,.66f),2);
            Box("Pipe",new Vector2(-8,3.6f),new Vector2(.13f,1.9f),new Color(.52f,.40f,.26f),2);
            Box("Pipe branch",new Vector2(-7.7f,3.4f+Floor*.3f),new Vector2(.6f,.1f),new Color(.52f,.40f,.26f),2);
            if(Floor==1) {
                Box("Outside",new Vector2(0,-4.25f),new Vector2(17.7f,1.5f),new Color(.12f,.20f,.24f),1);
                Box("Threshold",new Vector2(0,-3.45f),new Vector2(17.7f,.12f),new Color(.5f,.51f,.43f),2);
                DrawPerson(new Vector2(-4,-4),new Color(.5f,.42f,.31f),"Landlord");
                Box("Empty door frame",new Vector2(0,-3.35f),new Vector2(2,.16f),new Color(.56f,.42f,.26f),3);
                entryFrame=worldObjects[worldObjects.Count-1].transform;
            } else if(Floor==3) DrawPerson(new Vector2(3,4),new Color(.38f,.35f,.44f),"Neighbour");
            if(Floor==3){Box("Parcel shelf",new Vector2(3,4.4f),new Vector2(1,.35f),Accent,2);}
            Box("Crooked picture",new Vector2(5.7f,2.8f),new Vector2(.6f,.44f),new Color(.42f,.37f,.27f),3);
            worldObjects[worldObjects.Count-1].transform.rotation=Quaternion.Euler(0,0,State.Threat>=2?12:0);
            if(State.Threat>=2)foreach(int sign in new[]{-1,1}) {
                Box("Water mark",new Vector2(sign*3+.2f,2.98f),new Vector2(.1f,.42f),new Color(.14f,.23f,.25f),4);
                if(doorFrameSprite!=null&&doorLeafSprite!=null)
                    waterMarkRenderers.Add(worldObjects[worldObjects.Count-1].GetComponent<SpriteRenderer>());
            }
            foreach(var mark in marks) if(mark.Key.StartsWith(Floor+":")) {
                Box("Chalk",mark.Value,new Vector2(.3f,.1f),new Color(.9f,.88f,.7f),3);
                Box("Chalk cross",mark.Value,new Vector2(.1f,.3f),new Color(.9f,.88f,.7f),3);
            }
            person=DrawPlayer(Position);
            RefreshDoorSorting();
        }
        void BuildDoorVisual(string side,Vector2 position,bool open)
        {
            var root=new GameObject("Door visual "+side); root.transform.position=position; worldObjects.Add(root);
            var frame=new GameObject("Door frame"); frame.transform.SetParent(root.transform,false);
            frame.transform.localPosition=Vector3.zero; frame.transform.localScale=Vector3.one;
            var frameRenderer=frame.AddComponent<SpriteRenderer>(); frameRenderer.sprite=doorFrameSprite;
            doorRenderers.Add(frameRenderer);
            var leaf=new GameObject("Door leaf"); leaf.transform.SetParent(root.transform,false);
            leaf.transform.localPosition=Vector3.zero; leaf.transform.localScale=Vector3.one;
            var leafRenderer=leaf.AddComponent<SpriteRenderer>(); leafRenderer.sprite=doorLeafSprite; leafRenderer.enabled=!open;
            doorRenderers.Add(leafRenderer);
        }
        void RefreshDoorSorting()
        {
            int order=Position.y<=2.45f?5:8;
            foreach(var renderer in doorRenderers) if(renderer!=null) renderer.sortingOrder=order;
            foreach(var renderer in waterMarkRenderers) if(renderer!=null) renderer.sortingOrder=order+1;
        }
        Transform DrawPlayer(Vector2 p)
        {
            if(playerSprite==null)return DrawPerson(p,new Color(.33f,.48f,.43f),"Player");
            var root=new GameObject("Player"); root.transform.position=p;
            root.transform.localScale=Vector3.one*1.2f;
            var sr=root.AddComponent<SpriteRenderer>(); sr.sprite=playerSprite; sr.sortingOrder=7;
            worldObjects.Add(root);
            return root.transform;
        }
        Transform DrawPerson(Vector2 p,Color coat,string name)
        {
            var root=new GameObject(name); root.transform.position=p; worldObjects.Add(root);
            Box(name+" feet",p+new Vector2(0,-.2f),new Vector2(.38f,.3f),new Color(.055f,.065f,.07f),5);
            var feet=worldObjects[worldObjects.Count-1]; feet.transform.SetParent(root.transform,true);
            Box(name+" coat",p,new Vector2(.46f,.62f),coat,6);
            worldObjects[worldObjects.Count-1].transform.SetParent(root.transform,true);
            Box(name+" head",p+new Vector2(0,.4f),new Vector2(.35f,.3f),new Color(.69f,.61f,.48f),7);
            worldObjects[worldObjects.Count-1].transform.SetParent(root.transform,true);
            return root.transform;
        }
        bool DoorOpen(string side) => doors.TryGetValue(Floor+":"+side,out var v)&&v;

        void Update()
        {
            if(!Ready)return;
            if(Input.GetKeyDown(KeyCode.Escape)&&Input.compositionString.Length==0) { if(Panel=="") OpenPanel("pause"); else if(Panel!="intro") ClosePanel(); }
            bool editingNotes=Panel=="archive"&&(GUI.GetNameOfFocusedControl()=="notes"||Input.compositionString.Length>0);
            if(!State.Dead&&!IsOpening&&!editingNotes) {
                if(Input.GetKeyDown(KeyCode.J)) TogglePanel("archive");
                if(Input.GetKeyDown(KeyCode.Tab)) TogglePanel("items");
            }
            if(Panel==""&&!State.Dead&&!IsOpening) {
                Vector2 d=new Vector2((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),
                    (Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));
                TryMove(d,Time.deltaTime);
                if(Input.GetKeyDown(KeyCode.E)) InteractNearest();
                if(Input.GetKeyDown(KeyCode.Alpha2)) {lamp=!lamp; Notify(lamp?"手电筒亮了。":"你关掉了手电筒。");}
                if(Input.GetKeyDown(KeyCode.Alpha1)) LeaveMark();
            }
            Tick(Time.deltaTime);
        }
        void LateUpdate()
        {
            if(!Ready)return;
            // Preserve world scale while following the player; never zoom out to fit the whole floor.
            float halfWidth=worldCamera.orthographicSize*worldCamera.aspect;
            float xLimit=Mathf.Max(0,9.5f-halfWidth);
            // Edge breathing room keeps the player above the bottom HUD.
            float yLimit=Mathf.Max(0,7f-worldCamera.orthographicSize);
            worldCamera.transform.position=new Vector3(
                Mathf.Clamp(Position.x,-xLimit,xLimit),
                Mathf.Clamp(Position.y,-yLimit,yLimit),-10);
        }
        public void Tick(float seconds)
        {
            State.Paused=Panel!="";
            State.Inside=Floor!=1||Position.y>=-3.45f;
            State.Advance(seconds);
            AudioListener.pause=State.Paused||State.Dead;
            if(!State.Paused&&!State.Dead) {
                toastTime=Mathf.Max(0,toastTime-seconds);
                if(!initialAnomaly&&State.Inside) {
                    initialAnomaly=true;
                    nextKnock=State.Time;
                }
                if(initialAnomaly&&Floor==1&&State.Inside&&knocksPlayed==3&&State.Time>=nextKnock+15)knocksPlayed=0;
                if(initialAnomaly&&Floor==1&&State.Inside&&knocksPlayed<3&&State.Time>=nextKnock) {
                    knocksPlayed++; nextKnock=State.Time+.45; knockPulse=1;
                    audioSource.PlayOneShot(knock,.8f);
                    Notify("身后传来轻响，空门框颤了一下。");
                    if(knocksPlayed==3)Record("first-knock","入户后，空着的门框响了三声，木边轻轻震动。");
                }
                knockPulse=Mathf.Max(0,knockPulse-seconds*3);
                if(Floor==1&&entryFrame!=null)entryFrame.position=new Vector3(Mathf.Sin((float)State.Time*70)*.06f*knockPulse,-3.35f,0);
                if(opening>0) { opening-=seconds; if(opening<=0) FinishDoor(); }
            }
        }
        bool OnStairs(Vector2 p)=>p.x<=-6.4f||p.x>=6.4f;

        public bool TryMove(Vector2 direction,float seconds)
        {
            if(Panel!=""||State.Dead||IsOpening)return false;
            Vector2 next=Position+Vector2.ClampMagnitude(direction,1)*2.6f*seconds;
            next.x=Mathf.Clamp(next.x,-8.5f,8.5f);
            float minY=Floor==1?-4.7f:-3f,maxY=4.5f;
            if(OnStairs(next)&&next.x<=-6.4f&&Floor<3)next.y=Mathf.Clamp(next.y,minY,5.2f);       // west flight: walk up across floors
            else if(OnStairs(next)&&next.x>=6.4f&&Floor>1)next.y=Mathf.Clamp(next.y,-5.2f,maxY); // east flight: walk down across floors
            else next.y=Mathf.Clamp(next.y,minY,maxY);
            if(!CanTraverse(Position,next)) {
                var horizontal=new Vector2(next.x,Position.y);
                var vertical=new Vector2(Position.x,next.y);
                if(CanTraverse(Position,horizontal))next=horizontal;
                else if(CanTraverse(Position,vertical))next=vertical;
                else next=Position;
            }
            Position=next; person.position=Position; RefreshDoorSorting();
            if(Floor<3&&Position.x<=-6.4f&&Position.y>4.55f){ChangeFloor(1,new Vector2(Position.x,-3f+(Position.y-4.5f)));return true;}
            if(Position.x>=6.4f&&Floor>1&&Position.y<-3.05f){ChangeFloor(-1,new Vector2(Position.x,4.5f-(-3f-Position.y)));return true;}
            State.Inside=Floor!=1||Position.y>=-3.45f;
            return true;
        }
        void ChangeFloor(int delta,Vector2 entry)
        {
            Floor+=delta; Position=entry; person.position=Position;
            State.Inside=true;
            Record("stairs:"+Floor,"这一段层间楼梯分成两跑，各有十级台阶。墙边管线有不同的分支。");
            BuildWorld(); Notify("你走过两跑楼梯。记住自己经过的路。");
        }
        bool CanTraverse(Vector2 from,Vector2 to)
        {
            if(to.y>2.45f&&Mathf.Abs(to.x)<6.4f&&!(to.x>.7f&&to.x<5.3f)&&!(to.x> -5.3f&&to.x<-.7f))return false;
            bool crosses=(from.y<=2.45f&&to.y>2.45f)||(from.y>2.45f&&to.y<=2.45f);
            if(crosses) {
                if(Mathf.Abs(to.x)>=6.4f||Mathf.Abs(from.x)>=6.4f)return true; // stairwell is open at the partition gap
                float x=Mathf.Lerp(from.x,to.x,(2.45f-from.y)/(to.y-from.y));
                return (Math.Abs(x+3)<.5f&&DoorOpen("west"))||(Math.Abs(x-3)<.5f&&DoorOpen("east"));
            }
            return true;
        }
        string Nearest()
        {
            string nearest=""; float distance=1.35f;
            Action<string,Vector2> consider=(id,p)=>{float d=Vector2.Distance(Position,p);if(d<distance){distance=d;nearest=id;}};
            consider("window",new Vector2(0,2.2f));
            consider("west",new Vector2(-3,2.2f)); consider("east",new Vector2(3,2.2f));
            if(Floor<3)consider("up",new Vector2(-7,0));
            if(Floor>1)consider("down",new Vector2(7,0));
            if(Floor==1){consider("landlord",new Vector2(-4,-4));consider("exit",new Vector2(0,-4.5f));}
            if(Floor==3&&DoorOpen("east")&&Position.y>3.5f)consider("neighbour",new Vector2(3,4));
            return nearest;
        }
        public string NearestTarget => Nearest();
        public void InteractNearest()
        {
            if(Panel!=""||State.Dead||IsOpening)return;
            string id=Nearest();
            if(id=="up"||id=="down") {
                Floor+=id=="up"?1:-1; Position=new Vector2(id=="up"?-6:6,0);
                State.Inside=true;
                Record("stairs:"+Floor,"这一段层间楼梯分成两跑，各有十级台阶。墙边管线有不同的分支。");
                BuildWorld(); Notify("你走过两跑楼梯。记住自己经过的路。"); return;
            }
            if(id=="window") { Record("window:"+Floor,WindowFact()); OpenPanel("window");return; }
            if(id=="landlord") {
                dialogueKind="landlord";
                dialogue=Parcel?"拿到了就好。雨还很大，路上小心。":"你的旧包裹寄在三层东户。愿意上去取吗？\n不急，你也可以先看看，或者改天再来。";
                OpenPanel("dialogue");return;
            }
            if(id=="neighbour") {
                dialogueKind="neighbour"; dialogue="我没敲过门。刚才明明是楼下的声音……\n可我一直在屋里。你也听到了？\n\n"+(Parcel?"包裹已经交给你了。":"你的包裹在这里，核对名字就可以取走。");
                Record("testimony","邻居说自己没有敲门，并称声音来自楼下。这只是他的说法。");
                OpenPanel("dialogue");return;
            }
            if(id=="exit") { OpenPanel("exit");return; }
            if(id=="east"||id=="west") {
                if(DoorOpen(id)){doors[Floor+":"+id]=false;BuildWorld();Notify("门合上了。");return;}
                pendingDoor=id; declaration=1; OpenPanel("door");
            }
        }
        public void SetDeclaration(int value){if(value<1||value>3)throw new ArgumentOutOfRangeException(nameof(value));declaration=value;}
        public void ConfirmDoor()
        {
            if(Panel!="confirm")return;
            pendingAction="action-"+(++actionNumber); ClosePanel(); opening=.8f;
        }
        public void RequestDoorConfirmation(){if(Panel=="door")OpenPanel("confirm");}
        void FinishDoor()
        {
            opening=0;
            Record(pendingAction,"你确认了本次寻访意图，并打开了"+(pendingDoor=="east"?"东户":"西户")+"的门。");
            var outcome=State.OpenDoor(pendingAction,Floor,declaration,true);
            doors[Floor+":"+pendingDoor]=true; BuildWorld();
            if(outcome==DoorOutcome.Fatal){Notify("声音停了。");return;}
            if(outcome==DoorOutcome.Escalated){Notify(State.Threat==1?"视野染上暗红色。门后传来一声闷响。":"墙上的画歪了。门边出现了水痕。");audioSource.PlayOneShot(knock,1);}
            else Notify("门被推开。屋里很安静。");
        }
        string WindowFact()=>Floor==1?"窗外的固定屋顶明显高于窗台，中间隔着两排窗。":Floor==2?"窗外的固定屋顶高于窗台，中间隔着一排窗。":"窗外的固定屋顶与窗台大致平齐。";
        public void LeaveMark()
        {
            if(Panel!=""||State.Dead||IsOpening)return;
            Vector2 p=new Vector2(Mathf.Round(Position.x*2)/2,Mathf.Round(Position.y*2)/2);
            string key=Floor+":"+p.x+":"+p.y;
            if(!marks.ContainsKey(key)){marks[key]=p;Record("mark:"+key,"你用粉笔在走过的位置留下了短线。标记不会替你判断楼层。");BuildWorld();}
            Notify("粉笔短线留在原处。");
        }
        void Record(string id,string text){
            string key=id.StartsWith("window:")?id:id.StartsWith("action-")?"door:"+Floor+":"+pendingDoor:"";
            string source="";
            if(key!="") {
                if(!sourceNames.TryGetValue(key,out source)){source=(key.StartsWith("window:")?"观察窗":"寻访门")+" · 记录对象"+(sourceNames.Count+1);sourceNames.Add(key,source);}
                source+="：";
            }
            State.Observe(id,"["+TimeSpan.FromSeconds(State.Time).ToString(@"mm\:ss")+"] "+source+text);
        }
        public void CollectParcel(){
            if(Panel!="dialogue"||dialogueKind!="neighbour"||Parcel)return;
            Parcel=true;Record("parcel","住户核对姓名后，将旧包裹交给了你。");ClosePanel();Notify("你取回了包裹。可以离开，也可以继续观察。");
        }
        public bool HasObservation(string text){foreach(var fact in State.Facts)if(fact.Contains(text))return true;return false;}
        public bool HasMark(int floor,Vector2 position){foreach(var mark in marks)if(mark.Key.StartsWith(floor+":")&&mark.Value==position)return true;return false;}
        public void OpenPanel(string name){Panel=name;State.Paused=true;}
        public void ClosePanel(){Panel="";State.Paused=false;}
        void TogglePanel(string name){if(Panel==name)ClosePanel();else if(Panel==""||Panel=="archive"||Panel=="items")OpenPanel(name);}
        public void AcceptMotive(bool yes){Accepted=yes;if(yes)Record("motive","房东说，你的旧包裹寄在三层东户。");ClosePanel();}
        void Notify(string text){toast=text;toastTime=6;}
        public void ResetRun(){UnityEngine.SceneManagement.SceneManager.LoadScene(0);}

        void Styles()
        {
            if(label!=null)return;
            label=new GUIStyle(GUI.skin.label){font=font,fontSize=19,wordWrap=true};label.normal.textColor=Paper;
            small=new GUIStyle(label){fontSize=15};small.normal.textColor=Muted;
            heading=new GUIStyle(label){fontSize=27,fontStyle=FontStyle.Bold};
            button=new GUIStyle(GUI.skin.button){font=font,fontSize=18,wordWrap=true};
            field=new GUIStyle(GUI.skin.textArea){font=font,fontSize=18,wordWrap=true};
        }
        void Fill(Rect r,Color c){Color prev=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=prev;}
        void Text(float x,float y,float w,float h,string value,GUIStyle style=null){GUI.Label(new Rect(x,y,w,h),value,style??label);}
        bool Button(float x,float y,float w,string text){return GUI.Button(new Rect(x,y,w,40),text,button);}
        void ScreenOverlay(Color color)
        {
            var previous=GUI.matrix;GUI.matrix=Matrix4x4.identity;
            Fill(new Rect(0,0,Screen.width,Screen.height),color);
            GUI.matrix=previous;
        }
        void OnGUI()
        {
            if(!Ready)return;
            Styles();scale=Mathf.Min(Screen.width/1366f,Screen.height/768f);
            var previous=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1366*scale)/2,(Screen.height-768*scale)/2),Quaternion.identity,Vector3.one*scale);
            try {
                if(!lamp)ScreenOverlay(new Color(0,0,0,.12f));
                if(State.Threat>0&&!State.Dead&&State.Inside) {
                    float alpha=.035f+.018f*Mathf.Sin((float)State.Time*2);
                    ScreenOverlay(new Color(.35f,.06f,.025f,alpha));
                }
                if(State.Dead){DrawDeath();return;}
                if(Panel!=""){DrawPanel();return;}

                WorldLabel(new Vector2(-3,3.25f),"西户");WorldLabel(new Vector2(3,3.25f),"东户");
                WorldLabel(new Vector2(0,3.4f),"窗");
                if(Floor<3)WorldLabel(new Vector2(-7,1.1f),"上行楼梯");
                if(Floor>1)WorldLabel(new Vector2(7,1.1f),"下行楼梯");
                if(Floor==1){WorldLabel(new Vector2(-4,-3.25f),"房东");WorldLabel(new Vector2(0,-4.8f),"街道出口");}

                var backing=new Color(.035f,.052f,.058f,.88f);
                Fill(new Rect(24,24,210,64),backing);
                Fill(new Rect(24,24,3,64),Muted);
                Text(40,32,182,25,"普通来访者");
                Text(40,60,182,23,"状态 · "+(State.Threat>0?"不安":"清醒"),small);
                Fill(new Rect(1022,24,320,64),backing);
                Text(1038,32,288,25,"旧居民楼");
                Text(1038,60,288,23,Parcel?"包裹已取回 · 去留由你":Accepted?"来意 · 取回寄存的旧包裹":"雨夜来访 · 自由探索",small);

                string target=Nearest();
                string prompt=IsOpening?"正在推门……":target!=""?"E  交互":"";
                if(prompt!=""||toastTime>0) {
                    Fill(new Rect(363,610,640,66),backing);
                    if(prompt!="")Text(381,618,604,25,prompt);
                    if(toastTime>0)Text(381,prompt!=""?647:631,604,27,toast,small);
                }
                Fill(new Rect(263,690,840,56),backing);
                bool enabled=GUI.enabled;
                GUI.enabled=enabled&&!IsOpening;
                if(Button(275,698,170,"1  粉笔标记"))LeaveMark();
                if(Button(455,698,170,"2  手电 · "+(lamp?"亮":"灭"))){
                    lamp=!lamp;Notify(lamp?"手电筒亮了。":"你关掉了手电筒。");
                }
                if(Button(645,698,132,"J  档案"))TogglePanel("archive");
                if(Button(787,698,152,"Tab  物品"))TogglePanel("items");
                GUI.enabled=enabled;
                if(Button(949,698,142,"Esc  暂停"))OpenPanel("pause");
            } finally {GUI.matrix=previous;}
        }
        void WorldLabel(Vector2 p,string value)
        {
            // Name nearby, visible landmarks only; labels are not an off-screen navigation aid.
            if(Vector2.Distance(Position,p)>2.8f)return;
            Vector3 screen=worldCamera.WorldToScreenPoint(p);
            if(screen.z<=0||screen.x<0||screen.x>Screen.width||screen.y<0||screen.y>Screen.height)return;
            float x=(screen.x-(Screen.width-1366*scale)/2)/scale;
            float y=(Screen.height-screen.y-(Screen.height-768*scale)/2)/scale;
            float width=small.CalcSize(new GUIContent(value)).x+20;
            var rect=new Rect(x-width/2,y-14,width,28);
            if(rect.xMin<24||rect.xMax>1342||rect.yMin<106||rect.yMax>596)return;
            Fill(rect,new Color(.04f,.065f,.075f,.82f));
            GUI.Label(new Rect(rect.x+10,rect.y,width-10,28),value,small);
        }
        void DrawPanel()
        {
            ScreenOverlay(new Color(.025f,.035f,.04f,.88f));
            Fill(new Rect(285,125,795,510),new Color(.09f,.12f,.13f));
            Text(315,143,690,40,Panel=="archive"?"灵异档案":Panel=="items"?"随身物品":Panel=="window"?"窗外观察":Panel=="door"||Panel=="confirm"?"寻访住户":Panel=="pause"?"暂停":Panel=="exit"?"离开居民楼":"雨夜来访",heading);
            Text(865,151,195,25,"世界已暂停",small);
            if(Panel=="intro") {
                Text(315,219,715,190,"你来到一栋老居民楼。房东在门口等雨停。\n\n这是一份普通人调查原型：没有战斗，也没有必须完成的委托。\n用 WASD 行走，靠近后按 E 交互。打开档案可记录自己的猜测。\n\n本版重开只用于测试，不代表正式游戏可以回退死亡结果。");
                if(Button(315,555,230,"进入雨夜"))ClosePanel();
            } else if(Panel=="door"||Panel=="confirm") {
                Text(315,215,715,80,"你要找的是哪一层的住户？\n选择表达本次寻访意图，不会校验你的判断。");
                if(Panel=="door") {
                    for(int i=1;i<=3;i++)if(Button(315+(i-1)*235,327,215,(declaration==i?"●  ":"")+i+" 层住户"))SetDeclaration(i);
                    if(Button(315,478,280,"继续确认"))RequestDoorConfirmation();
                } else {
                    Text(315,330,715,95,"你准备按“"+declaration+"层住户”的意图打开"+(pendingDoor=="east"?"东户":"西户")+"。\n确认后开始推门，动作可暂停，不能中途撤销。");
                    if(Button(315,478,280,"确认并推门"))ConfirmDoor();
                }
                if(Button(615,478,230,"取消，先观察"))ClosePanel();
            } else if(Panel=="archive") {
                Text(315,200,690,30,"已知事实 / 来源：现场观察或已注明的证词",small);
                scroll=GUI.BeginScrollView(new Rect(315,237,710,190),scroll,new Rect(0,0,680,Math.Max(190,State.Facts.Count*65)));
                for(int i=0;i<State.Facts.Count;i++)GUI.Label(new Rect(4,i*65,662,63),"· "+State.Facts[i],label);
                GUI.EndScrollView();
                Text(315,441,650,28,"个人推测（不会自动判断对错）",small);
                GUI.SetNextControlName("notes");
                State.Notes=GUI.TextArea(new Rect(315,478,710,92),State.Notes??"",2000,field);
                if(Button(835,580,190,"返回观察"))ClosePanel();
            } else if(Panel=="items") {
                Text(315,210,710,65,"物品按情境使用。携带不受负重或总格数限制。",small);
                if(Button(315,300,330,"手电筒 · "+(lamp?"关闭":"打开"))){lamp=!lamp;ClosePanel();}
                if(Button(665,300,330,"粉笔 · 在脚边留下记号")) {ClosePanel();LeaveMark();}
                Text(315,376,710,85,"旧纸条：三层东户寄存着你的包裹。\n"+(Parcel?"旧包裹：已经取回。":"随身钥匙：自己的住处钥匙，打不开这里的门。"));
                if(Button(315,552,200,"收起物品"))ClosePanel();
            } else if(Panel=="window") {
                Text(315,203,720,60,WindowFact());
                Fill(new Rect(345,292,620,210),new Color(.11f,.18f,.22f));
                Fill(new Rect(640,325,240,177),new Color(.24f,.3f,.31f));
                Fill(new Rect(620,320,275,8),Accent);
                for(int row=0;row<3;row++)for(int col=0;col<3;col++)Fill(new Rect(675+col*62,349+row*47,24,25),new Color(.48f,.51f,.40f));
                float sill=320+(3-Floor)*55;
                Fill(new Rect(370,sill,175,8),Paper);
                Text(377,sill+15,200,28,"眼前的窗台",small);Text(650,288,240,26,"对面的固定屋顶",small);
                Text(315,514,690,36,"只呈现观察到的相对关系。你可以记在自己的笔记里。",small);
                if(Button(315,566,230,"收回视线"))ClosePanel();
            } else if(Panel=="dialogue") {
                Text(315,223,710,205,dialogue);
                if(dialogueKind=="landlord"&&!Parcel) {
                    if(Button(315,505,290,"我上去取包裹"))AcceptMotive(true);
                    if(Button(625,505,290,"暂时不取，先看看"))AcceptMotive(false);
                } else {
                    if(dialogueKind=="neighbour"&&!Parcel&&Button(315,475,290,"核对姓名，取回包裹"))CollectParcel();
                    if(Button(315,530,230,"结束交谈"))ClosePanel();
                }
            } else if(Panel=="exit") {
                Text(315,235,710,175,Parcel?"你带着旧包裹站在雨里。\n楼里的事情未必已经结束。":Accepted?"你暂时离开了楼栋。包裹还在那里。\n这也是一个有效选择。":"你没有接受委托，仍然可以随时回来观察。");
                Text(315,425,710,55,"继续本局会保留已发生的变化与档案。重开仅用于原型测试。",small);
                if(Button(315,534,300,"留在街道 / 继续本局"))ClosePanel();
                if(Button(635,534,300,"退出原型"))Application.Quit();
            } else if(Panel=="pause") {
                Text(315,234,710,110,"模拟时间与事件已暂停。\n本版关闭后不会恢复当前测试局；正式存档在后续阶段验证。");
                if(Button(315,432,300,"继续"))ClosePanel();
                if(Button(635,432,300,"重开测试局"))ResetRun();
                if(Button(315,493,300,"退出原型"))Application.Quit();
            }
        }
        void DrawDeath()
        {
            ScreenOverlay(new Color(.04f,.03f,.035f,.96f));
            Text(315,150,740,55,"这段人生结束了。",heading);
            Text(315,220,740,65,"回顾只保留你实际观察到的事情。不会公布未发现的规则。",small);
            int start=Math.Max(0,State.Facts.Count-4);
            for(int i=start;i<State.Facts.Count;i++)Text(315,306+(i-start)*54,740,52,"· "+State.Facts[i]);
            if(Button(315,574,300,"重开测试局（原型专用）"))ResetRun();
            if(Button(635,574,250,"退出"))Application.Quit();
        }
    }
}
