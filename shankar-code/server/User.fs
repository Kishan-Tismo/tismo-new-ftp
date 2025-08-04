module User
open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Security.Cryptography
open Microsoft.AspNetCore.Cryptography.KeyDerivation
open Microsoft.IdentityModel.Tokens
open Types

let HashIterationCount = 120000; // as per OWASP 2021 recommendation for PBKDF2-HMAC-SHA512
let HashSize = 32; //  bytes
let SaltSize = 16; // bytes - IV needs to be 16 bytes for AES encoding

let jwtKey =   
    let byteKey: byte [] = Array.zeroCreate 128
    use rng = RandomNumberGenerator.Create()
    rng.GetBytes(byteKey);    
    byteKey
 

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
      
      
let HashPassword plainTextPassword =
    let salt = GetSalt()
    let hash = DoHash salt plainTextPassword
    hash, (Convert.ToBase64String salt)
    
let VerifyHashPassword hashedPassword salt plainPassword =
    let computedHash = DoHash (Convert.FromBase64String salt) plainPassword
    computedHash = hashedPassword
    
let doVerification (settings:Settings) (username:string) (plainTextPassword:string) =    
   if username = String.Empty || plainTextPassword = String.Empty then false
   elif settings.ClientUserName <> username &&
        settings.TismoUserName <> username then false
   else
       let hashpwd, salt = if settings.ClientUserName = username
                            then settings.ClientHashedPassword, settings.ClientSalt
                            else settings.TismoHashedPassword, settings.TismoSalt                      
       VerifyHashPassword hashpwd salt plainTextPassword 

let tokenExpiryTime = 8 // in hours
    
// Returns JWT serialized token if user authentication is successful
// otherwise returns empty string    
let Authenticate (settings:Settings) (username:string) (plainTextPassword:string) =        
    try      
       if doVerification settings username plainTextPassword then           
           let claims = [| Claim(ClaimTypes.Name, username)
                           Claim(ClaimTypes.Role, "role")    // TODO  role ?                   
                        |]
           let tokenDescriptor = SecurityTokenDescriptor(
                    Subject = ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(tokenExpiryTime),
                    SigningCredentials = SigningCredentials( SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha256Signature)
                )
           let tokenHandler =  JwtSecurityTokenHandler()
           tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor))           
       else           
           String.Empty                                
    with e ->
        Logger.log $"Error: authenticating {username} - {e.Message}"
        String.Empty
       