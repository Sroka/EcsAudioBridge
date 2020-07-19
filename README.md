# EcsAudioBridge
Unity AudioSource pooling system usable from ECS

## How to use
In order to use just add EcsAudioSource component to object. It will automatically add AudioSource and read all data from it

To play audio from ECS system just call Play() or PlayOneShot() method on a component:

```
            Entities.ForEach((ref EcsAudioSource ecsAudioSource, ) =>
            {
                if (/* Some condition */)
                {
                    ecsAudioSource.PlayOneShot();
                }
            }).Schedule();
```

```
            Entities.ForEach((ref EcsAudioSource ecsAudioSource, ) =>
            {
                if (/* Some condition */)
                {
                    ecsAudioSource.Play();
                }
            }).Schedule();
 ```


