﻿namespace Nu
open System
open System.Numerics
open FSharp.Core
open Prime

/// Provides for time-driven behavior.
type 'a Behavior = GameTime -> 'a

[<RequireQualifiedAccess>]
module Behavior =

    let returnB (a : 'a): 'a Behavior =
        let bhvr = fun _ -> a
        bhvr

    let map<'a, 'b> mapper (bhvr : 'a Behavior) : 'b Behavior =
        bhvr >> mapper

    let apply (bhvrF : ('a -> 'b) Behavior) (bhvrA : 'a Behavior) : 'b Behavior =
        let bhvr = fun time ->
            let a = bhvrA time
            let f = bhvrF time
            f a
        bhvr

    let bind (bhvr : 'a Behavior) (f : 'a -> 'b Behavior) : 'b Behavior =
        let bhvr =
            fun time ->
                let a = bhvr time
                let b = f a
                b time
        bhvr

    let lift1<'a, 'b>
        (op : 'a -> 'b)
        (bhvr : 'a Behavior) :
        Behavior<'b> =
        map op bhvr

    let lift2<'a, 'b, 'c>
        (op : 'a -> 'b -> 'c)
        (bhvr : 'a Behavior)
        (bhvr2 : 'b Behavior) :
        'c Behavior =
        let bhvr = fun time ->
            let a = bhvr time
            let b = bhvr2 time
            op a b
        bhvr

    let lift3<'a, 'b, 'c, 'd>
        (op : 'a -> 'b -> 'c -> 'd)
        (bhvr : 'a Behavior)
        (bhvr2 : 'b Behavior)
        (bhvr3 : 'c Behavior) :
        'd Behavior =
        let bhvr = fun time ->
            let a = bhvr time
            let b = bhvr2 time
            let c = bhvr3 time
            op a b c
        bhvr

    let lift4<'a, 'b, 'c, 'd, 'e>
        (op : 'a -> 'b -> 'c -> 'd -> 'e)
        (bhvr : 'a Behavior)
        (bhvr2 : 'b Behavior)
        (bhvr3 : 'c Behavior)
        (bhvr4 : 'd Behavior) :
        'e Behavior =
        let bhvr = fun time ->
            let a = bhvr time
            let b = bhvr2 time
            let c = bhvr3 time
            let d = bhvr4 time
            op a b c d
        bhvr

    let lift5<'a, 'b, 'c, 'd, 'e, 'f>
        (op : 'a -> 'b -> 'c -> 'd -> 'e -> 'f)
        (bhvr : 'a Behavior)
        (bhvr2 : 'b Behavior)
        (bhvr3 : 'c Behavior)
        (bhvr4 : 'd Behavior)
        (bhvr5 : 'e Behavior) :
        'f Behavior =
        let bhvr = fun time ->
            let a = bhvr time
            let b = bhvr2 time
            let c = bhvr3 time
            let d = bhvr4 time
            let e = bhvr5 time
            op a b c d e
        bhvr

    let inline loop stride bounce bhvr =
        map (fun a ->
            let local = a % stride
            if bounce then
                if int (a / stride) % 2 = 0
                then local
                else stride - local
            else local)
            bhvr

    let inline slice start length bhvr =
        map (fun a ->
            let local = a - start
            if local > length then length
            elif local < Generics.zero () then Generics.zero ()
            else local)
            bhvr

    let inline normalize (length : GameTime) bhvr =
        map (fun (time : GameTime) ->
            let length = length.Seconds
            let time = time.Seconds
            time / length)
            bhvr

    // basic behavior combinators
    let id bhvr = returnB bhvr
    let constant k bhvr = map (constant k) bhvr
    let not bhvr = map not bhvr
    let fst bhvr = map fst bhvr
    let snd bhvr = map snd bhvr
    let dup bhvr = map dup bhvr
    let prepend a bhvr = map (fun b -> (a, b)) bhvr
    let append b bhvr = map (fun a -> (a, b)) bhvr
    let withFst a bhvr = map (fun (_, b) -> (a, b)) bhvr
    let withSnd b bhvr = map (fun (a, _) -> (a, b)) bhvr
    let mapFst mapper bhvr = map (fun (a, b) -> (mapper a, b)) bhvr
    let mapSnd mapper bhvr = map (fun (a, b) -> (a, mapper b)) bhvr
    let swap bhvr = map (fun (a, b) -> (b, a)) bhvr

    // boot-strapping combinators
    let unit = let (bhvr : unit Behavior) = fun _ -> () in bhvr
    let time : GameTime Behavior = let bhvr = fun (time : GameTime) -> time in bhvr
    let timeLoopRaw stride bounce = loop stride bounce time
    let timeSliceRaw start length = slice start length time
    let timeLoop stride bounce = let bhvr = timeLoopRaw stride bounce in normalize stride bhvr
    let timeSlice start length = let bhvr = timeSliceRaw start length in  normalize length bhvr

    // advanced behavior combinators
    let inline eq b bhvr = map (fun a -> a = b) bhvr
    let inline neq b bhvr = map (fun a -> a <> b) bhvr
    let inline lerp a b bhvr = map (fun p -> a + p * (b - a)) bhvr
    let inline step stride bhvr = map (fun p -> int (p / stride)) bhvr
    let inline pulse stride bhvr = map (fun steps -> steps % 2 <> 0) (step stride bhvr)
    let inline random bhvr = map (fun a -> Random(hash a)) bhvr
    let inline randomb bhvr = map (fun a -> Random(hash a).Next() <= Int32.MaxValue / 2) bhvr
    let inline randomi bhvr = map (fun a -> Random(hash a).Next()) bhvr
    let inline randoml bhvr = map (fun a -> Random(hash a).NextInt64()) bhvr
    let inline randomf bhvr = map (fun a -> Random(hash a).NextDouble() |> single) bhvr
    let inline randomd bhvr = map (fun a -> Random(hash a).NextDouble()) bhvr
    let inline sum summand bhvr = map (fun a -> a + summand) bhvr
    let inline delta difference bhvr = map (fun a -> a - difference) bhvr
    let inline scale scalar bhvr = map (fun a -> a * scalar) bhvr
    let inline ratio divisor bhvr = map (fun a -> a / divisor) bhvr
    let inline sin bhvr = map sin bhvr
    let inline cos bhvr = map cos bhvr
    let inline powf n bhvr = map (fun a -> single (Math.Pow (double a, double n))) bhvr
    let inline powd n bhvr = map (fun a -> Math.Pow (a, n)) bhvr
    let inline pow2 n bhvr = map (fun a -> Vector2.Pow (a, n)) bhvr
    let inline pow3 n bhvr = map (fun a -> Vector3.Pow (a, n)) bhvr
    let inline pow4 n bhvr = map (fun a -> Vector4.Pow (a, n)) bhvr
    let inline pow2i n bhvr = map (fun a -> Vector2i.Pow (a, n)) bhvr
    let inline pow3i n bhvr = map (fun a -> Vector3i.Pow (a, n)) bhvr
    let inline pow4i n bhvr = map (fun a -> Vector4i.Pow (a, n)) bhvr
    let inline powc n bhvr = map (fun a -> Color.Pow (a, n)) bhvr
    let inline pown n bhvr = map (fun a -> pown a n) bhvr
    let inline isZero bhvr = map Generics.isZero bhvr
    let inline notZero bhvr = map Generics.notZero bhvr
    let inline isNeg bhvr = map (fun a -> a < Generics.zero ()) bhvr
    let inline isPositive bhvr = map (fun a -> a < Generics.zero ()) bhvr

    let inline serp (scale : (^a * single) -> ^a) (a : ^a) (b : ^a) bhvr : ^a Behavior =
        map (fun (s : single) ->
            let scaled = float s * Math.PI * 2.0
            let scalar = Math.Sin scaled
            a + scale (b - a, single scalar))
            bhvr

    let inline serpScaled (scale : (^a * single) -> ^a) (a : ^a) (b : ^a) (scalar : single) bhvr : ^a Behavior =
        map (fun (s : single) ->
            let scaled = float s * Math.PI * 2.0 * float scalar
            let scalar = Math.Sin scaled
            a + scale (b - a, single scalar))
            bhvr

    let inline cerp (scale : (^a * single) -> ^a) (a : ^a) (b : ^a) bhvr : ^a Behavior =
        map (fun (s : single) ->
            let scaled = float s * Math.PI * 2.0
            let scalar = Math.Cos scaled
            a + scale (b - a, single scalar))
            bhvr

    let inline cerpScaled (scale : (^a * single) -> ^a) (value : ^a) (value2 : ^a) (scalar : single) bhvr : ^a Behavior =
        map (fun (progress : single) ->
            let scaled = float progress * Math.PI * 2.0 * float scalar
            let scalar = Math.Cos scaled
            value + scale (value2 - value, single scalar))
            bhvr

    let inline ease (scale : (^a * single) -> ^a) (a : ^a) (b : ^a) bhvr : ^a Behavior =
        map (fun (s : single) ->
            let scalar = single (Math.Pow (Math.Sin (Math.PI * double s * 0.5), 2.0))
            a + scale (b - a, scalar))
            bhvr

    let inline easeIn (scale : (^a * single) -> ^a) (a : ^a) (b : ^a) bhvr : ^a Behavior =
        map (fun (s : single) ->
            let scaled = float s * Math.PI * 0.5
            let scalar = single (1.0 + Math.Sin (scaled + Math.PI * 1.5))
            a + scale (b - a, scalar))
            bhvr

    let inline easeOut (scale : (^a * single) -> ^a) (a : ^a) (b : ^a) bhvr : ^a Behavior =
        map (fun (s : single) ->
            let scaled = float s * Math.PI * 0.5
            let scalar = single (Math.Sin scaled)
            a + scale (b - a, scalar))
            bhvr

    let inline serpf a b bhvr = serp (fun (c, s) -> c * s) a b bhvr
    let inline serpScaledf a b scalar bhvr = serpScaled (fun (c, s) -> c * s) a b scalar bhvr
    let inline cerpf a b bhvr = cerp (fun (c, s) -> c * s) a b bhvr
    let inline cerpScaledf a b scalar bhvr = cerpScaled (fun (c, s) -> c * s) a b scalar bhvr
    let inline easef a b bhvr = ease (fun (c, s) -> c * s) a b bhvr
    let inline easeInf a b bhvr = easeIn (fun (c, s) -> c * s) a b bhvr
    let inline easeOutf a b bhvr = easeIn (fun (c, s) -> c * s) a b bhvr

    // product behavior combinators
    let inline product (bhvr : 'a Behavior) (bhvr2 : 'b Behavior) = let (bhvr3 : Behavior<'a * 'b>) = fun a -> (bhvr a, bhvr2 a) in bhvr3
    let inline map2 mapper (bhvr : 'a Behavior) (bhvr2 : 'b Behavior) : 'c Behavior = product bhvr bhvr2 |> map mapper
    let inline mapProduct mapper bhvr = map (fun (a, b) -> mapper a b) bhvr
    let inline sumProduct bhvr = mapProduct (fun a b -> a + b) bhvr
    let inline deltaProduct bhvr = mapProduct (fun a b -> a - b) bhvr
    let inline scaleProduct bhvr = mapProduct (fun a b -> a * b) bhvr
    let inline ratioProduct bhvr = mapProduct (fun a b -> a / b) bhvr
    let inline orProduct bhvr = mapProduct (||) bhvr
    let inline norProduct bhvr = mapProduct (fun a b -> Operators.not a && Operators.not b) bhvr
    let inline xorProduct bhvr = mapProduct (fun a b -> a <> b) bhvr
    let inline andProduct bhvr = mapProduct (fun a b -> a && b) bhvr
    let inline nandProduct bhvr = mapProduct (fun a b -> Operators.not (a && b)) bhvr

/// Builds behaviors.
type [<Sealed>] BehaviorBuilder () =
    member this.Return a = Behavior.returnB a
    member this.ReturnFrom a = a
    member this.Bind (a, f) = Behavior.bind a f

[<AutoOpen>]
module BehaviorBuilder =

    /// Builds behaviors.
    let behave = BehaviorBuilder ()

[<RequireQualifiedAccess>]
module GameTimeExtension =
    type GameTime with
        member this.Run (behavior : 'a Behavior) =
            behavior this