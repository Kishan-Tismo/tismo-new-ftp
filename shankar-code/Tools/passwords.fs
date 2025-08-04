module Passwords
open System
open System.Security.Cryptography
open Microsoft.AspNetCore.Cryptography.KeyDerivation

let HashIterationCount = 120000; // as per OWASP 2021 recommendation for PBKDF2-HMAC-SHA512
let HashSize = 32; //  bytes
let SaltSize = 16; // bytes - IV needs to be 16 bytes for AES encoding

let genPwd(psize) = 
    use rng = RandomNumberGenerator.Create()
    let mutable password = Array.zeroCreate<byte> psize
    rng.GetBytes(password)
    let charArray = "abcdefghijkmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ23456789" 
    password |> Array.map(fun b ->( int b) % (charArray.Length))
             |> Array.map(fun i -> charArray[i])
             |> String
    
    
let createPlainTextPassword() =
    genPwd(4) + "-" + genPwd(4) + "-" + genPwd(4) + "-" + genPwd(4)
    

let GetSalt() =
    use rng = RandomNumberGenerator.Create()
    let mutable salt = Array.zeroCreate<byte> SaltSize
    rng.GetBytes(salt)
    salt


let DoHash (salt:byte[]) (plainPassword:string) =
      let hash = KeyDerivation.Pbkdf2( plainPassword, salt,
                         KeyDerivationPrf.HMACSHA512,
                         HashIterationCount,
                         HashSize)
      Convert.ToBase64String hash


let HashPassword plainTextPassword = // returns hash and the salt
    let salt = GetSalt()
    let hash = DoHash salt plainTextPassword
    hash, (Convert.ToBase64String salt)

let VerifyHashPassword hashedPassword salt plainPassword =
    let computedHash = DoHash (Convert.FromBase64String salt) plainPassword
    computedHash = hashedPassword