using UnityEngine;

namespace Mod
{
    public class Mod
    {
        public static void Main()
        {
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Metal Cube"),
                    NameOverride = "Firetruck",
                    DescriptionOverride = "A truck with fire.",
                    CategoryOverride = ModAPI.FindCategory("Vehicles"),
                    ThumbnailOverride = ModAPI.LoadSprite("preview.png"),
                    AfterSpawn = (Instance) =>
                    {
                        //TRUCK BODY
                        Rigidbody2D rb = Instance.GetComponent<Rigidbody2D>();
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("truckBodyCol.png");
                        GameObject.Destroy(Instance.GetComponent<BoxCollider2D>());
                        Instance.gameObject.FixColliders();
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("truckBody.png");
                        Instance.GetComponent<SpriteRenderer>().sortingOrder=0;
                        float direction = Instance.transform.localScale.x;
                        rb.mass = 5000f;

                        //WHEELS
                        GameObject w = ModAPI.FindSpawnable("Wheel").Prefab;

                        Vector2[] wps = { new Vector2(-2.9f, -1.3f), new Vector2(3.7f, -1.3f) };

                        Firetruck truck = Instance.AddComponent<Firetruck>();
                        truck.objects = new GameObject[19];
                        CarBehaviour car = Instance.AddComponent<CarBehaviour>();
                        car.WheelJoints = new WheelJoint2D[2];

                        truck.wheels = new WheelJoint2D[2];

                        for (int i = 0; i < 2; i++)
                        {
                            GameObject wheel = GameObject.Instantiate(w, Instance.transform.position + new Vector3(wps[i].x * direction, wps[i].y, 0f), Quaternion.identity);
                            wheel.transform.localScale *= 1.5f;
                            wheel.GetComponent<Rigidbody2D>().mass = 150f;
                            wheel.GetComponent<SpriteRenderer>().sortingOrder = 1;
                            WheelJoint2D wj = Instance.AddComponent<WheelJoint2D>();
                            wj.connectedBody = wheel.GetComponent<Rigidbody2D>();
                            wj.anchor = wps[i];
                            wj.autoConfigureConnectedAnchor = true;
                            JointSuspension2D js = wj.suspension;
                            js.dampingRatio = 0.75f;
                            js.frequency = 5f;
                            wj.suspension = js;
                            wj.breakForce = 15000f;
                            car.WheelJoints[i] = wj;
                            truck.wheels[i] = wj;
                            truck.objects[i]=wheel;
                        }

                        //HOSE
                        HingeJoint2D hoseBody = GameObject.Instantiate(ModAPI.FindSpawnable("Plastic Barrel").Prefab, Instance.transform.position + new Vector3(0.35f*direction, -0.6f, 0f), Quaternion.identity).AddComponent<HingeJoint2D>();
                        hoseBody.transform.localScale = new Vector3(direction, 1f, 1f);
                        hoseBody.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("hoseBody.png");
                        hoseBody.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        hoseBody.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.1f);
                        hoseBody.GetComponent<Rigidbody2D>().mass = 0.1f;
                        hoseBody.connectedBody = rb;
                        hoseBody.anchor = new Vector2(-0.25f, 0f);
                        truck.objects[2] = hoseBody.gameObject;

                        HingeJoint2D lastBody = hoseBody;

                        for (int i = 3; i < 16; i++)
                        {
                            HingeJoint2D currentBody = GameObject.Instantiate(ModAPI.FindSpawnable("Plastic Barrel").Prefab, Instance.transform.position + new Vector3((0.35f + (0.6f*(i-2)))*direction, -0.6f, 0f), Quaternion.identity).AddComponent<HingeJoint2D>();
                            currentBody.gameObject.layer = 13;//colliding debris layer, using this as a means of preventing the extinguisher particles from colliding with this
                            currentBody.breakForce = 10000f;
                            currentBody.transform.localScale = new Vector3(direction, 1f, 1f);
                            currentBody.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("hoseBody.png");
                            currentBody.GetComponent<SpriteRenderer>().sortingOrder = 1;
                            currentBody.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.1f);
                            currentBody.GetComponent<Rigidbody2D>().mass = 0.1f;
                            currentBody.connectedBody = lastBody.GetComponent<Rigidbody2D>();
                            currentBody.anchor = new Vector2(-0.25f, 0f);

                            foreach (Collider2D c in Instance.GetComponents<Collider2D>())
                            {
                                Physics2D.IgnoreCollision(currentBody.GetComponent<BoxCollider2D>(), c, true);
                            }
                            for (int j = 0; j < i-1; j++)
                            {
                                Physics2D.IgnoreCollision(currentBody.GetComponent<BoxCollider2D>(), truck.objects[j].GetComponent<Collider2D>(), true);
                            }

                            truck.objects[i] = currentBody.gameObject;
                            lastBody = currentBody;
                        }

                        //HOSE HEAD
                        HingeJoint2D hoseHead = GameObject.Instantiate(ModAPI.FindSpawnable("Metal Cube").Prefab, Instance.transform.position + new Vector3(8.7f*direction, -0.6f, 0f), Quaternion.identity).AddComponent<HingeJoint2D>();
                        hoseHead.gameObject.layer=13;
                        hoseHead.breakForce=10000f;
                        hoseHead.transform.localScale = new Vector3(direction, 1f, 1f);
                        hoseHead.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("hoseHead.png");
                        hoseHead.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        hoseHead.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.1f);
                        hoseHead.GetComponent<Rigidbody2D>().mass = 0.5f;
                        hoseHead.connectedBody = lastBody.GetComponent<Rigidbody2D>();
                        hoseHead.anchor = new Vector2(-0.25f, 0f);
                        truck.objects[16] = hoseHead.gameObject;

                        foreach (Collider2D c in Instance.GetComponents<Collider2D>())
                        {
                            Physics2D.IgnoreCollision(hoseHead.GetComponent<Collider2D>(), c, true);
                        }
                        for (int i = 0; i < 16; i++)
                        {
                            Physics2D.IgnoreCollision(hoseHead.GetComponent<Collider2D>(), truck.objects[i].GetComponent<Collider2D>(), true);
                        }

                        //HOSE BEHAVIOUR
                        FlamethrowerBehaviour e = GameObject.Instantiate(ModAPI.FindSpawnable("Fire Extinguisher").Prefab, Vector3.zero, Quaternion.identity).GetComponent<FlamethrowerBehaviour>();
                        FlamethrowerBehaviour extinguisher = hoseHead.gameObject.AddComponent<FlamethrowerBehaviour>();
                        GameObject prt = GameObject.Instantiate(e.particlePrefab, Vector3.up*-10000, Quaternion.identity);
                        var main = prt.GetComponent<ParticleSystem>().main;
                        main.startSize=2.5f;
                        main.startSpeed=1.5f;
                        var sz = prt.GetComponent<ParticleSystem>().sizeOverLifetime;
                        sz.enabled = false;
                        var col = prt.GetComponent<ParticleSystem>().collision;
                        col.collidesWith = 5 << 9;//collides with the bounds and objects but nothing else (layer 9 is object and 11 is bounds, 5 in binary is 101 so bitshifted to the left 9 times will enable the 9th and the 11th)

                        extinguisher.particlePrefab=prt;
                        extinguisher.Effect = e.Effect;
                        extinguisher.LayerMask = e.LayerMask;
                        extinguisher.Point = e.Point;
                        extinguisher.Range = e.Range*10;
                        extinguisher.Angle = e.Angle;
                        
                        GameObject.Destroy(e.gameObject);

                        Transform muzzle = new GameObject().transform;
                        muzzle.parent=hoseHead.transform;
                        muzzle.localPosition=Vector3.zero;
                        extinguisher.Muzzle=muzzle;

                        Shooter shooter=hoseHead.gameObject.AddComponent<Shooter>();
                        shooter.direction=direction;
                        hoseHead.gameObject.AddComponent<UseEventTrigger>().Action = () => { shooter.shoot=true; };

                        AudioSource asrc = hoseHead.gameObject.AddComponent<AudioSource>();
                        asrc.loop = true;
                        asrc.clip = ModAPI.LoadSound("hose.wav");
                        shooter.asrc = asrc;

                        //HOLDER
                        JointAngleLimits2D jointZero = new JointAngleLimits2D();
                        jointZero.min = 0; jointZero.max = 0;

                        HingeJoint2D holder = GameObject.Instantiate(w, Instance.transform.position + new Vector3(0.29f*direction, 0.35f, 0f), Quaternion.identity).AddComponent<HingeJoint2D>();
                        holder.connectedBody = rb;
                        holder.limits = jointZero;
                        holder.GetComponent<SpriteRenderer>().sprite = null;
                        holder.GetComponent<CircleCollider2D>().radius = 0.1f;
                        truck.objects[17]=holder.gameObject;

                        //SMOKE
                        Transform smoke = GameObject.Instantiate(ModAPI.FindSpawnable("Particle Projector").Prefab, Instance.transform.position + new Vector3(-1.75f*direction, -1.5f, 0f), Quaternion.identity).transform;
                        smoke.GetComponent<SpriteRenderer>().sprite = null;
                        smoke.transform.GetChild(0).GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Decals");//make the smoke render above the truck body
                        smoke.GetComponent<Rigidbody2D>().isKinematic=true;
                        GameObject.Destroy(smoke.GetComponent<Collider2D>());
                        smoke.parent=Instance.transform;//make it activate when the truck is

                        //FIRE EXTINGUISHER
                        HingeJoint2D fireExtinguisher = GameObject.Instantiate(ModAPI.FindSpawnable("Fire Extinguisher").Prefab, Instance.transform.position + new Vector3(-5.75f * direction, -0.15f, 0f), Quaternion.identity).AddComponent<HingeJoint2D>();
                        fireExtinguisher.transform.localScale=new Vector3(-direction, 1f, 1f);
                        fireExtinguisher.connectedBody = rb;
                        fireExtinguisher.breakForce=500f;
                        JointAngleLimits2D limits = new JointAngleLimits2D();
                        limits.min = -5; limits.max = 5;
                        fireExtinguisher.limits = limits;
                        truck.objects[18] = fireExtinguisher.gameObject;

                        //CAR BEHAVIOUR
                        car.MotorSpeed = -3000f;
                        car.Activated = false;
                        car.Phys = Instance.GetComponent<PhysicalBehaviour>();
                        car.IsBrakeEngaged = true;

                        truck.car = car;
                        truck.source=Instance.AddComponent<AudioSource>();
                        truck.loop=ModAPI.LoadSound("loop.wav");
                        truck.start=ModAPI.LoadSound("start.mp3");
                        truck.stop=ModAPI.LoadSound("stop.mp3");
                        truck.source.loop=true;
                        truck.source.volume=0.75f;
                        truck.source.minDistance=0.1f;
                        truck.source.maxDistance=1f;
                    }
                }
            );
        }
    }

    public class Shooter : MonoBehaviour
    {
        public bool shoot=false;

        public float direction=1f;

        public AudioSource asrc;

        bool playing = false;
        
        void Update()
        {
            if (shoot)
            {
                Rigidbody2D r = GameObject.Instantiate(ModAPI.FindSpawnable("Soap").Prefab, transform.position + transform.right, Quaternion.identity).GetComponent<Rigidbody2D>();
                r.gameObject.layer=13;
                r.GetComponent<SpriteRenderer>().sprite=null;
                r.GetComponent<PhysicalBehaviour>().Properties.SlidingLoop=null;//remove sound when sliding
                r.GetComponent<PhysicalBehaviour>().Wetness=1000000f;//help extinguish fires
                r.AddForce(transform.right*0.75f*direction, ForceMode2D.Impulse);
                GameObject.Destroy(r.gameObject, 1f);

                if(!playing){asrc.Play();playing=true;}

                if (Input.GetKeyUp(KeyCode.F)) { shoot = false; }
            }
            else{asrc.Stop(); playing=false;}
        }
    }

    public class Firetruck : MonoBehaviour
    {
        public GameObject[] objects;

        public WheelJoint2D[] wheels;

        public CarBehaviour car;

        public AudioSource source;
        public AudioClip loop;
        public AudioClip start;
        public AudioClip stop;

        bool looping = false;
        bool stopped = false;
        bool started = false;

        float speed = 20f;

        void Update()
        {
            if (car != null && car.Activated)
            {
                if(!started)
                {
                    source.clip = start;
                    source.Play();
                    source.loop = false;
                    stopped = false;
                    started = true;
                }
                
                if (speed < 250f) { speed+=0.05f; }

                foreach (WheelJoint2D w in wheels)
                {
                    JointMotor2D jm = w.motor;
                    jm.maxMotorTorque = speed;
                    w.motor = jm;
                }

                if (!looping && speed > 30f)//the speed > 50 is just a means of giving the stat sfx time to play before the loop starts.
                {
                    source.clip=loop;
                    source.Play();
                    source.loop=true;
                    looping = true;
                }
            }
            else if(!stopped)
            {
                source.clip=stop;
                source.Play();
                source.loop=false;
                looping = false;
                started=false;
                stopped=true;
                speed = 20f;
            }
        }

        void OnDestroy()
        {
            foreach (GameObject o in objects) { GameObject.Destroy(o); }
        }
    }
}