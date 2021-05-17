using Unity.Entities;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using Unity.Physics;
using Unity.Physics.Systems;


public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject gameObjectPrefab;
    private List<Entity> cubes;
    [SerializeField] int xSize = 10;
    [SerializeField] int ySize = 10;
    [SerializeField] int zSize = 10;
    [Range(1f, 2f)]
    [SerializeField] float spacing = 1f;
    [SerializeField] float NDelay = 1f;
    [SerializeField]  bool isMoving = true;
    [SerializeField] float Speed = 1;
    float time = 0;
    private Entity entityPrefab;
    private World defaultWorld;
    private EntityManager entityManager;

    void Start()
    {

        defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultWorld.EntityManager;
        cubes = new List<Entity>();

        if (gameObjectPrefab != null)
        {

            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
            entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);


            InstantiateEntityGrid(xSize, ySize, zSize, spacing);
        }
    }

    Entity RayCast(float3 from, float3 to)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        RaycastInput raycastInput = new RaycastInput
        {
            Start = from,
            End = to,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0,
            }
        };
        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        if (collisionWorld.CastRay(raycastInput, out hit))
        {
            Entity hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
            float3 x = entityManager.GetComponentData<Translation>(hitEntity).Value;

            return hitEntity;
        }
        else
        {
            return Entity.Null;
        }
    }
    private void Update()
    {
        if(!isMoving)
        {
            time += (float) Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(0))
        {
            time += Time.deltaTime;
            UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float rayDist = 100f;
            Entity destroyedEntity = RayCast(ray.origin, ray.direction * rayDist);
            if (destroyedEntity != Entity.Null)
            {

                float3 PosDestrEnt = entityManager.GetComponentData<Translation>(destroyedEntity).Value;
                for (int i = 0; i < cubes.Count; i++)
                {
                    if (cubes[i] == destroyedEntity)
                    {
                        cubes[i] = Entity.Null;
                    }
                }
                entityManager.DestroyEntity(destroyedEntity);
                StartCoroutine(CreateWithDelay(NDelay, PosDestrEnt));
            }
        }
    }

    IEnumerator CreateWithDelay(float delay, float3 PosDestrEnt)
    {
        yield return new WaitForSeconds(delay);
        InstantiateEntity(PosDestrEnt);
    }


    private void InstantiateEntity(float3 position)
    {
        if (entityManager == null)
        {
            Debug.LogWarning("InstantiateEntity WARNING: No EntityManager found!");
            return;
        }

        Entity myEntity = entityManager.Instantiate(entityPrefab);

        MaterialColor mc = new MaterialColor
        {
            Value = new float4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f))
        };
        entityManager.SetComponentData<MaterialColor>(myEntity, mc);

        entityManager.AddComponentData(myEntity, new PhysicsCollider
        {
            Value = Unity.Physics.BoxCollider.Create(new Unity.Physics.BoxGeometry() {
                Center = float3.zero, 
                Orientation = quaternion.identity, 
                Size = new float3(1, 1, 1) }, 
                Unity.Physics.CollisionFilter.Default)
        });
        entityManager.SetComponentData(myEntity, new Translation
        {
            Value = position
        });

        entityManager.SetComponentData(myEntity, new zOffset
        {
            Offset = position.z
        });
        entityManager.SetComponentData(myEntity, new MSpeed
        {
            Value = Speed,
            isMoving = isMoving
        });
        entityManager.SetComponentData(myEntity, new TimeComp
        {
            Value = time
        });
        bool check = true;
        for (int i = 0; i < cubes.Count; i++)
        {
            if (cubes[i] == Entity.Null)
            {
                check = false;
                cubes[i] = myEntity;
            }
        }
            if(check)
            {
                cubes.Add(myEntity);
            }
        
    }
    public void Stop()
    {
        isMoving = !isMoving;
        for (int i = 0; i < cubes.Count; i++)
        {
            if (cubes[i] != Entity.Null)
            {
                //Entity myEntity = entityManager.Instantiate(cubes[i]);
                entityManager.SetComponentData(cubes[i], new MSpeed
                {
                    Value = Speed,
                    isMoving = isMoving
                });
                entityManager.SetComponentData(cubes[i], new TimeComp
                {
                    Value = time
                });
                //entityManager.DestroyEntity(cubes[i]);
                //cubes[i] = myEntity;
            }
        }
    }


    private void InstantiateEntityGrid(int dimX, int dimY, int dimZ, float spacing = 1f)
    {
        for (int i = 0; i < dimX; i++)
        {
            for (int j = 0; j < dimY; j++)
            {
                for (int k = 0; k < dimZ; k++)
                {
                    InstantiateEntity(new float3(i * spacing, j * spacing, k * spacing));
                }
            }
        }
    }
}
