module Password
open System;
open System.Security.Cryptography;
open System.Threading
open Microsoft.AspNetCore.Cryptography.KeyDerivation;


//  hash, salt = HashPassword plainTextPassword
//               both hash and salt are base64 encoded strings/
//
// To verify a user entered password is correct, 
//    VerifyPassword  hash  salt userEnteredPlainTextPassword
//               hash -- hashed password for the user 
//               salt --  salt for the user 
// both hash and salt are base64 encoded strings


// If there are too many wrong passwords in sequence,
// it is possible that someone is trying to hack in to the system 
// in that case, introduce a small delay that will increase the time required
// for a brute force attack


let private HashIterationCount = 120000; // as per OWASP 2021 recommendation for PBKDF2-HMAC-SHA512
let private HashSize = 32; //  bytes
let private SaltSize = 16; // bytes 

let private  DoHash  (salt:byte[])  (plainPassword:string) =
    KeyDerivation.Pbkdf2(
         plainPassword,
         salt,
         KeyDerivationPrf.HMACSHA512,
         HashIterationCount,
         HashSize)
    |> Convert.ToBase64String


let private GetSalt() =
    let salt = Array.zeroCreate SaltSize
    use  rng = RandomNumberGenerator.Create()
    rng.GetBytes(salt)
    salt


        
let HashPassword ( plainTextPassword:string) =
    if plainTextPassword.Length = 0 then
        raise (ArgumentNullException "plainTextPassword")
    let salt = GetSalt();
    let hash = DoHash salt plainTextPassword
    (hash, Convert.ToBase64String(salt))


let VerifyHashPassword  (hashedPasswordBase64:string)   (saltBase64:string)  (plainPassword:string) =
    if plainPassword.Length = 0
    then false
    else hashedPasswordBase64 = DoHash (Convert.FromBase64String(saltBase64)) plainPassword       
        
