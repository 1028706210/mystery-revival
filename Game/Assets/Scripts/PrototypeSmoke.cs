using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MysteryRevival
{
    // Runs only on explicit --smoke launches. Never substitutes for blind playtesting.
    public sealed class PrototypeSmoke : MonoBehaviour
    {
        PrototypeGame game;
        int checks;
        bool captureFailed;
        IEnumerator Start()
        {
            game=GetComponent<PrototypeGame>();
            yield return null;
            try { CheckInitialDoors(); CheckCorridorWindowWorld("initial"); PartOne(); }
            catch(Exception e){Finish(false,e.ToString());yield break;}
            yield return null;
            try { CheckStableWorldInstances("part-one"); }
            catch(Exception e){Finish(false,e.ToString());yield break;}
            if(!Application.isBatchMode) {
                yield return new WaitForEndOfFrame(); Capture("exploration.png");
            }
            game.OpenPanel("archive");
            yield return null;
            if(!Application.isBatchMode){yield return new WaitForEndOfFrame();Capture("archive.png");}
            game.ClosePanel();
            try { PartTwo(); }
            catch(Exception e){Finish(false,e.ToString());yield break;}
            yield return null;
            try { CheckStableWorldInstances("part-two"); }
            catch(Exception e){Finish(false,e.ToString());yield break;}
            if(!Application.isBatchMode){yield return new WaitForEndOfFrame();Capture("death.png");}
            Finish(!captureFailed,captureFailed?"Visual capture failed; interaction checks passed.":"");
        }
        void Check(bool value,string name){if(!value)throw new Exception("FAILED: "+name);checks++;Debug.Log("SMOKE_PASS "+name);}
        static bool Near(float actual,float expected){return Mathf.Abs(actual-expected)<.001f;}
        List<GameObject> CurrentWorldObjects()
        {
            var field=typeof(PrototypeGame).GetField("worldObjects",BindingFlags.Instance|BindingFlags.NonPublic);
            if(field==null)throw new Exception("FAILED: current-world-list-field");
            var objects=field.GetValue(game) as List<GameObject>;
            if(objects==null)throw new Exception("FAILED: current-world-list-value");
            return objects;
        }
        GameObject CurrentRoot(string name)
        {
            GameObject found=null;
            foreach(var obj in CurrentWorldObjects()) {
                if(obj==null||!obj.activeInHierarchy||obj.scene.handle!=game.gameObject.scene.handle||obj.transform.parent!=null||obj.name!=name)continue;
                if(found!=null)throw new Exception("FAILED: current-root-unique-"+name);
                found=obj;
            }
            if(found==null)throw new Exception("FAILED: current-root-present-"+name);
            return found;
        }
        SpriteRenderer DoorRenderer(string side,string child)
        {
            var node=CurrentRoot("Door visual "+side).transform.Find(child);
            Check(node!=null,"door-"+side+"-"+child.ToLowerInvariant().Replace(' ','-')+"-child");
            var renderer=node.GetComponent<SpriteRenderer>();
            Check(renderer!=null,"door-"+side+"-"+child.ToLowerInvariant().Replace(' ','-')+"-renderer");
            return renderer;
        }
        void CheckSprite(SpriteRenderer renderer,string expectedName,string checkPrefix)
        {
            var sprite=renderer.sprite;
            Check(sprite!=null,checkPrefix+"-sprite-loaded");
            Check(sprite.name==expectedName,checkPrefix+"-resource-name");
            Check(Near(sprite.rect.width,128)&&Near(sprite.rect.height,160),checkPrefix+"-sprite-128x160");
            Check(sprite.texture!=null&&sprite.texture.width==128&&sprite.texture.height==160,checkPrefix+"-texture-128x160");
            Check(Near(sprite.pixelsPerUnit,100),checkPrefix+"-ppu-100");
            Check(Near(sprite.pivot.x,64)&&Near(sprite.pivot.y,8),checkPrefix+"-pivot-bottom-center");
        }
        void CheckDoorState(string side,bool leafVisible,int sortingOrder,string phase)
        {
            var frame=DoorRenderer(side,"Door frame");
            var leaf=DoorRenderer(side,"Door leaf");
            Check(frame.enabled,phase+"-"+side+"-frame-visible");
            Check(leaf.enabled==leafVisible,phase+"-"+side+"-leaf-visible-"+leafVisible);
            Check(frame.sortingOrder==sortingOrder,phase+"-"+side+"-frame-order-"+sortingOrder);
            Check(leaf.sortingOrder==sortingOrder,phase+"-"+side+"-leaf-order-"+sortingOrder);
        }
        void CheckInitialDoors()
        {
            var player=CurrentRoot("Player").GetComponent<SpriteRenderer>();
            Check(player!=null&&player.sortingOrder==7,"initial-player-order-7");
            foreach(string side in new[]{"west","east"}) {
                var root=CurrentRoot("Door visual "+side);
                float x=side=="west"?-3:3;
                Check(Near(root.transform.position.x,x)&&Near(root.transform.position.y,2.45f),"initial-"+side+"-root-position");
                Check(Vector3.Distance(root.transform.localScale,Vector3.one)<.001f,"initial-"+side+"-root-scale-one");
                var frame=DoorRenderer(side,"Door frame");
                var leaf=DoorRenderer(side,"Door leaf");
                Check(Vector3.Distance(frame.transform.localPosition,Vector3.zero)<.001f,"initial-"+side+"-frame-local-position-zero");
                Check(Vector3.Distance(leaf.transform.localPosition,Vector3.zero)<.001f,"initial-"+side+"-leaf-local-position-zero");
                Check(Vector3.Distance(frame.transform.localScale,Vector3.one)<.001f,"initial-"+side+"-frame-scale-one");
                Check(Vector3.Distance(leaf.transform.localScale,Vector3.one)<.001f,"initial-"+side+"-leaf-scale-one");
                CheckSprite(frame,"door_game_frame","initial-"+side+"-frame");
                CheckSprite(leaf,"door_game_leaf","initial-"+side+"-leaf");
                CheckDoorState(side,true,5,"initial");
            }
            CheckStableWorldInstances("initial");
        }
        void CheckCorridorWindowWorld(string phase)
        {
            var root=CurrentRoot("Corridor window wall");
            Check(Near(root.transform.position.x,0)&&Near(root.transform.position.y,2.45f),phase+"-window-root-position");
            Check(Vector3.Distance(root.transform.localScale,Vector3.one)<.001f,phase+"-window-root-scale-one");
            var renderer=root.GetComponent<SpriteRenderer>();
            Check(renderer!=null,phase+"-window-renderer");
            Check(renderer.enabled,phase+"-window-visible");
            Check(renderer.sortingOrder==2,phase+"-window-order-2");
            var sprite=renderer.sprite;
            Check(sprite!=null,phase+"-window-sprite-loaded");
            Check(sprite.name=="corridor_window_wall",phase+"-window-resource-name");
            Check(sprite.texture!=null&&sprite.texture.width==192&&sprite.texture.height==176,phase+"-window-texture-192x176");
            Check(Near(sprite.rect.width,192)&&Near(sprite.rect.height,176),phase+"-window-sprite-192x176");
            Check(Near(sprite.pixelsPerUnit,100),phase+"-window-ppu-100");
            Check(Near(sprite.pivot.x,96)&&Near(sprite.pivot.y,8),phase+"-window-pivot-96-8");

            var expectedX=new[]{-8f,-6f,-5f,-4f,-2f,-1f,1f,2f,4f,5f,6f,8f};
            var partitions=new List<GameObject>();
            int oldWindows=0;
            foreach(var obj in CurrentWorldObjects()) {
                if(obj==null||!obj.activeInHierarchy||obj.scene.handle!=game.gameObject.scene.handle)continue;
                if(obj.name=="Window")oldWindows++;
                else if(obj.name=="Partition")partitions.Add(obj);
            }
            Check(oldWindows==0,phase+"-no-old-window");
            Check(partitions.Count==expectedX.Length,phase+"-twelve-partitions");
            foreach(float x in expectedX) {
                GameObject found=null;
                foreach(var partition in partitions)if(Near(partition.transform.position.x,x)) {
                    Check(found==null,phase+"-partition-x-unique-"+x.ToString("0"));
                    found=partition;
                }
                Check(found!=null,phase+"-partition-x-present-"+x.ToString("0"));
                Check(Near(found.transform.position.y,2.9f),phase+"-partition-y-2.9-x-"+x.ToString("0"));
                Check(Near(found.transform.localScale.x,.95f)&&Near(found.transform.localScale.y,.3f),phase+"-partition-size-x-"+x.ToString("0"));
                var partitionRenderer=found.GetComponent<SpriteRenderer>();
                Check(partitionRenderer!=null&&partitionRenderer.sortingOrder==1,phase+"-partition-order-1-x-"+x.ToString("0"));
            }
        }
        void CheckWindowChalkVisibility()
        {
            WalkExact(0,2.4f);
            game.LeaveMark();
            var markPosition=new Vector2(0,2.5f);
            Check(game.HasMark(1,markPosition),"window-foot-chalk-recorded-at-0-2.5");
            var windowRenderer=CurrentRoot("Corridor window wall").GetComponent<SpriteRenderer>();
            Check(windowRenderer!=null,"window-foot-window-renderer");
            int segments=0;
            foreach(var obj in CurrentWorldObjects()) {
                if(obj==null||!obj.activeInHierarchy||obj.scene.handle!=game.gameObject.scene.handle)continue;
                if(obj.name!="Chalk"&&obj.name!="Chalk cross")continue;
                if(!Near(obj.transform.position.x,markPosition.x)||!Near(obj.transform.position.y,markPosition.y))continue;
                segments++;
                string checkPrefix=obj.name=="Chalk"?"window-chalk":"window-chalk-cross";
                var renderer=obj.GetComponent<SpriteRenderer>();
                Check(renderer!=null,checkPrefix+"-renderer");
                Check(renderer.sortingOrder==3,checkPrefix+"-order-3");
                Check(renderer.sortingOrder>windowRenderer.sortingOrder,checkPrefix+"-above-wall");
            }
            Check(segments==2,"window-foot-two-chalk-segments");
            WalkExact(0,0);
        }
        void CheckStableWorldInstances(string phase)
        {
            int west=0,east=0,players=0,windowWalls=0,oldWindows=0,partitions=0;
            foreach(var root in game.gameObject.scene.GetRootGameObjects()) {
                if(!root.activeInHierarchy)continue;
                if(root.name=="Door visual west")west++;
                else if(root.name=="Door visual east")east++;
                else if(root.name=="Player")players++;
                else if(root.name=="Corridor window wall")windowWalls++;
                else if(root.name=="Window")oldWindows++;
                else if(root.name=="Partition")partitions++;
            }
            Check(west==1,phase+"-one-west-door-root");
            Check(east==1,phase+"-one-east-door-root");
            Check(players==1,phase+"-one-player-root");
            Check(windowWalls==1,phase+"-one-window-wall-root");
            Check(oldWindows==0,phase+"-no-old-window-root");
            Check(partitions==12,phase+"-twelve-partition-roots");
        }
        void CheckOpenEastDoorBoundary()
        {
            foreach(float x in new[]{2.55f,3.45f}) {
                WalkExact(x,2.35f);
                game.TryMove(Vector2.up,.08f);
                Check(game.Position.y>2.45f,"open-east-door-pass-x-"+x.ToString("0.00"));
                CheckDoorState("east",false,8,"room-side-x-"+x.ToString("0.00"));
                var player=CurrentRoot("Player").GetComponent<SpriteRenderer>();
                Check(player!=null&&player.sortingOrder==7,"room-side-player-order-7-x-"+x.ToString("0.00"));
                game.TryMove(Vector2.down,.08f);
                Check(game.Position.y<=2.45f,"open-east-door-return-x-"+x.ToString("0.00"));
                CheckDoorState("east",false,5,"corridor-side-x-"+x.ToString("0.00"));
            }
            foreach(float x in new[]{2.45f,3.55f}) {
                WalkExact(x,2.35f);
                game.TryMove(Vector2.up,.08f);
                Check(game.Position.y<=2.45f,"open-east-door-block-x-"+x.ToString("0.00"));
            }
        }
        void CheckWaterMarks(int doorOrder,int waterOrder,string phase)
        {
            var marks=new List<GameObject>();
            foreach(var obj in CurrentWorldObjects()) {
                if(obj!=null&&obj.activeInHierarchy&&obj.scene.handle==game.gameObject.scene.handle&&obj.name=="Water mark")marks.Add(obj);
            }
            Check(marks.Count==2,phase+"-two-water-marks");
            bool west=false,east=false;
            var player=CurrentRoot("Player").GetComponent<SpriteRenderer>();
            Check(player!=null&&player.sortingOrder==7,phase+"-player-order-7");
            foreach(var mark in marks) {
                var position=mark.transform.position;
                if(Near(position.x,-2.8f)&&Near(position.y,2.98f))west=true;
                if(Near(position.x,3.2f)&&Near(position.y,2.98f))east=true;
                Check(Near(mark.transform.localScale.x,.1f)&&Near(mark.transform.localScale.y,.42f),phase+"-water-mark-size");
                var renderer=mark.GetComponent<SpriteRenderer>();
                Check(renderer!=null&&renderer.enabled,phase+"-water-mark-visible");
                Check(renderer.sortingOrder==waterOrder,phase+"-water-order-"+waterOrder);
                Check(renderer.sortingOrder>doorOrder,phase+"-water-above-door");
                if(waterOrder<7)Check(renderer.sortingOrder<player.sortingOrder,phase+"-water-below-player");
            }
            Check(west&&east,phase+"-water-mark-positions");
            Check(DoorRenderer("west","Door leaf").sortingOrder==doorOrder,phase+"-west-door-order-"+doorOrder);
            Check(DoorRenderer("east","Door leaf").sortingOrder==doorOrder,phase+"-east-door-order-"+doorOrder);
        }
        void Capture(string file)
        {
            try {
            var screenshot=ScreenCapture.CaptureScreenshotAsTexture();
            if(screenshot==null) {captureFailed=true;Debug.LogError("VISUAL_CAPTURE_FAILED "+file);return;}
            File.WriteAllBytes(Path.Combine(OutputFolder(),file),screenshot.EncodeToPNG());
            Destroy(screenshot);
            } catch(Exception e){captureFailed=true;Debug.LogError("VISUAL_CAPTURE_FAILED "+file+": "+e.Message);}
        }
        void Walk(float x,float y)
        {
            Vector2 target=new Vector2(x,y);
            for(int i=0;i<1200&&Vector2.Distance(game.Position,target)>.06f;i++) {
                Vector2 delta=target-game.Position;
                game.TryMove(delta.normalized,Mathf.Min(.02f,delta.magnitude/2.6f));game.Tick(.02f);
            }
            Check(Vector2.Distance(game.Position,target)<.07f,"walk-to-target");
        }
        void WalkExact(float x,float y)
        {
            Walk(x,y);
            Vector2 target=new Vector2(x,y);
            Vector2 correction=target-game.Position;
            if(correction.sqrMagnitude>0)game.TryMove(correction.normalized,correction.magnitude/2.6f);
            Check(Vector2.Distance(game.Position,target)<.001f,"walk-to-exact-boundary-column");
        }
        void Open(string side,int declared)
        {
            Walk(side=="east"?3:-3,2.2f);game.InteractNearest();
            Check(game.Panel=="door","door-intent-dialog");
            game.SetDeclaration(declared);game.RequestDoorConfirmation();
            Check(game.Panel=="confirm","explicit-confirmation");
            game.ConfirmDoor();Check(game.IsOpening,"action-started");
            game.Tick(.81f);
        }
        void PartOne()
        {
            Check(game.Ready,"runtime-ready");Check(game.Panel=="intro","intro-visible");
            double t=game.State.Time;game.Tick(5);Check(game.State.Time==t,"intro-pauses-world");
            game.ClosePanel();CheckWindowChalkVisibility();Walk(-4,-4);game.InteractNearest();Check(game.Panel=="dialogue","motive-dialogue");
            game.AcceptMotive(false);Walk(0,0);Check(game.State.Inside,"refusal-keeps-exploration");
            Check(game.State.Facts.Count>0,"pre-trigger-anomaly-recorded");
            game.LeaveMark();Check(game.State.Threat==0,"ordinary-mark-safe");
            Walk(0,2.2f);Check(game.NearestTarget=="window","floor-one-nearest-window");
            CheckCorridorWindowWorld("floor-one");game.InteractNearest();Check(game.Panel=="window","window-observation");
            t=game.State.Time;game.Tick(10);Check(game.State.Time==t,"window-pauses-world");game.ClosePanel();
            Walk(-3,2.2f);game.InteractNearest();game.ClosePanel();Check(game.State.Threat==0,"cancel-does-not-trigger");
            Walk(-7,.5f);game.LeaveMark();Check(game.HasMark(1,new Vector2(-7,.5f)),"chalk-cross-on-steps");
            Walk(-7,4.35f);game.TryMove(Vector2.up,.12f);game.Tick(.12f);
            Check(game.Floor==2,"walk-up-stairs-crosses-floor");
            Walk(-6,0);Check(game.State.Inside,"stair-walk-stays-inside");
            Walk(0,2.2f);Check(game.NearestTarget=="window","floor-two-nearest-window");
            CheckCorridorWindowWorld("floor-two");game.InteractNearest();game.ClosePanel();
            Walk(-7,0);game.InteractNearest();Check(game.Floor==3,"stairs-up-two");
            Walk(0,2.2f);Check(game.NearestTarget=="window","floor-three-nearest-window");
            CheckCorridorWindowWorld("floor-three");game.InteractNearest();game.ClosePanel();
            game.TryMove(Vector2.up,.2f);
            Check(game.Position.y<=2.45f,"window-wall-blocks-upward-movement");
            Check(game.HasObservation("两排窗")&&game.HasObservation("一排窗")&&game.HasObservation("大致平齐"),"three-window-records-generated");
            Walk(-7,0);Walk(-7,4.5f);
            for(int i=0;i<40;i++){game.TryMove(Vector2.up,.05f);game.Tick(.05f);}
            Check(game.Floor==3,"top-floor-west-stairs-stay");
            Check(game.Position.y<=4.5f,"top-floor-clamp-boundary");
            Check(game.NearestTarget=="","top-floor-no-up-interact");
            Walk(-7,0);Walk(3,2.2f);
            game.InteractNearest();game.SetDeclaration(3);game.RequestDoorConfirmation();game.ConfirmDoor();game.Tick(.81f);
            Check(!game.IsOpening&&game.State.Threat==0,"top-floor-door-open-safe");
            CheckDoorState("east",false,5,"top-floor-east-open");
            CheckDoorState("west",true,5,"top-floor-west-independent");
            CheckOpenEastDoorBoundary();
            Walk(3,3.2f);Check(game.Position.y>2.45f,"top-floor-door-traversable");
            Walk(3,2.2f);game.InteractNearest();
            Walk(7,-2.9f);game.TryMove(Vector2.down,.12f);game.Tick(.12f);
            Check(game.Floor==2,"walk-down-stairs-crosses-floor");
            Walk(7,0);game.InteractNearest();Check(game.Floor==1,"return-route");
            Check(game.HasMark(1,Vector2.zero),"chalk-survives-floor-return");
            Walk(-4,-4);game.InteractNearest();game.AcceptMotive(true);Check(game.Accepted,"motive-accepted");
            Walk(-7,0);game.InteractNearest();Walk(-7,0);game.InteractNearest();
            Open("east",3);Check(!game.Parcel,"door-does-not-auto-grant-parcel");
            Walk(3,4);game.InteractNearest();Check(game.Panel=="dialogue","resident-dialogue");
            game.CollectParcel();Check(game.Parcel,"parcel-retrieval");game.CollectParcel();
            Check(!game.State.Dead&&game.State.Threat==0,"ordinary-route-survives");
            Walk(3,3.2f);game.InteractNearest(); // close from inside
            CheckDoorState("east",true,8,"top-floor-east-closed");
            for(int i=0;i<60;i++)game.TryMove(Vector2.down,.02f);
            Check(game.Position.y>2.45f,"closed-door-blocks-exit");
            for(int i=0;i<100;i++)game.TryMove(Vector2.left,.02f);
            Check(game.Position.x>.69f,"room-side-wall-blocks-crossing");
            Walk(3,3.2f);game.InteractNearest();game.SetDeclaration(3);game.RequestDoorConfirmation();game.ConfirmDoor();game.Tick(.81f);
            Walk(3,2.2f);
            CheckDoorState("east",false,5,"top-floor-east-reopened-corridor");
            game.State.Notes="这里只是猜测，不应触发规则。";
            game.OpenPanel("archive");t=game.State.Time;game.Tick(3);Check(t==game.State.Time,"archive-pauses-world");
            game.ClosePanel();Check(game.State.Threat==0,"notes-do-not-trigger");
        }
        void PartTwo()
        {
            Open("west",1);Check(game.State.Threat==1,"first-threat-feedback");
            game.InteractNearest(); // close same door
            Open("west",1);Check(game.State.Threat==1,"cooldown-action-not-queued");
            game.InteractNearest();game.Tick(20);Open("west",1);Check(game.State.Threat==2,"new-action-after-boundary");
            CheckWaterMarks(5,6,"threat-two-corridor");
            WalkExact(-3,3.2f);Check(game.Position.y>2.45f,"threat-two-crosses-open-west-door");
            CheckWaterMarks(8,9,"threat-two-room");
            WalkExact(-3,2.2f);Check(game.Position.y<=2.45f,"threat-two-returns-to-corridor");
            CheckWaterMarks(5,6,"threat-two-corridor-return");
            Walk(7,0);game.InteractNearest();
            Check(game.Floor==2,"threat-two-closed-floor");
            CheckDoorState("west",true,5,"threat-two-closed-floor");
            CheckDoorState("east",true,5,"threat-two-closed-floor");
            CheckWaterMarks(5,6,"threat-two-closed-floor");
            Walk(7,0);game.InteractNearest();Walk(0,-4.5f);
            Check(!game.State.Inside&&game.State.Threat==2,"exit-retains-threat");
            game.InteractNearest();Check(game.Panel=="exit","exit-summary");game.ClosePanel();
            game.Tick(60);Check(!game.State.Dead,"outside-no-automatic-death");
            Walk(-7,0);game.InteractNearest();Walk(-7,0);game.InteractNearest();
            Walk(-3,2.2f);game.InteractNearest(); // close west door from previous opening
            game.Tick(20);Open("west",1);Check(game.State.Dead,"terminal-state");
            double t=game.State.Time;game.Tick(30);Check(game.State.Time==t,"death-freezes-world");
        }
        static string OutputFolder()
        {
            string path=Environment.GetEnvironmentVariable("MYSTERY_SMOKE_OUTPUT");
            if(string.IsNullOrEmpty(path))path=Path.Combine(Application.persistentDataPath,"smoke");
            Directory.CreateDirectory(path);return path;
        }
        void Finish(bool success,string error)
        {
            string text="SMOKE_RESULT="+(success?"PASS":"FAIL")+" checks="+checks+"\n"+error;
            File.WriteAllText(Path.Combine(OutputFolder(),"result.txt"),text);
            if(success)Debug.Log(text);else Debug.LogError(text);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(success?0:1);
            #else
            Application.Quit(success?0:1);
            #endif
        }
    }
}
