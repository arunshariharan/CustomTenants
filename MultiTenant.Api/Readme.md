# Multi Tenant API with Membership functionality

## Implementation of Tenants in this API:
----------------------------------------
The multi-tenancy concept has been implemented by identifying the **host** making current request.
For the purposes of running it locally and on docker, the differentiation of tenants is based on **Port number**. Thre are 3 port numbers exposed and identified by the app - 1111, 2222 and 3333

To access Tenant A, hit `https://localhost:1111/swagger`(local) / `https://192.168.99.100:1111/swagger`(docker)
To access Tenant B, hit `https://localhost:2222/swagger`(local) / `https://192.168.99.100:2222/swagger`(docker)
To access Tenant C, hit `https://localhost:3333/swagger`(local) / `https://192.168.99.100:3333/swagger`(docker)

##### Note: The docker IP address could be different to the one mentioned here - replace it with your docker machine ip

## How to run the API
---------------------
### Docker:
* Navigate to https://hub.docker.com/r/arunshariharan/multitenant_api
* Copy the docker pull command or paste `docker pull arunshariharan/multitenant_api:final_release`.

```powershell
Make sure you got the latest image (check the tags). It could be different to the one mentioned above.
```

* Once downloaded, run the app using following command:

```powershell
docker run -p 1111:1111 -p 2222:2222 -p 3333:3333 arunshariharan/multitenant_api:final_release
```

* Once the app is up and running, you need to get the current IP address of your docker machine. This can be achieved by typing in `docker-machine ip` in a powershell. This is usually `192.168.99.100`
* Hit `https://192.168.99.100:1111/swagger` (or whatever the ip address of the machine was, accompanied by the **port number**)
* Each port number (1111/2222/3333) is a unique tenant and you can access each of these swaggers the same way as above: `https://192.168.99.100:2222/swagger`


### Locally:

* git clone locally to your system and open this project from VS 2017
* Make sure the startup is "MultiTenant.Api" and not IIS Server and Run the app
* The app opens swagger by default when you launch the application
* Once the app is up and running, you can hit the api through **Postman** or **Swagger** through `https://localhost:[PORT_NUMBER]`
* Each port number (1111/2222/3333) is a unique tenant and you can access each of these swaggers the same way as above: `https://localhost:2222/swagger`

## How to use the API
---------------------
##### This API employs *Token based authentication*. Hence every endpoint apart from `/api/signin` and `/api/newUser` requires a valid token to access.



First step would be is to **create a new user** according to swagger details, after which you can hit **Signin** endpoint and get a token.



The token comes with expiration (15 minutes) after which you are required to sign-in again for a fresh token.



Once you have a token, you need to enter it in the **Authorize** option on swagger / `Authorization: Bearer <token>` in postman header.



After this, you can hit other endpoints on not just the current tenant, but also other tenants. A signed in user in 1 tenant can access rest of the tenants too.
If you choose to signin rather than create user, a set of pre-seeded user data is given below which you can use to sign in.


## Features
-----------
* Signup / sign-in in one tenant, be able to hit endpoints on all tenants
* Ability to activate and de-activate user on current tenant only (Must be an admin to perform this operation)
* Ability to make someone an admin / remove from admin on current tenant only (Must be an admin to perform this operation)
* Update Password
* Retrieve all users on current tenant only
* Retrieve single user details on current tenant only

## Admin users
--------------

* By default, new users do not have admin permissions. In order to make someone an admin, you need to sign-in with existing user who has admin privileges.
* List of users are given below, with their passwords.

## Pre-seeded User data
Email Address | Password | Admin on Tenant | Deactive on Tenants
--------------|:--------:|:---------------:|-------------------:
user.a@test.com|12345678| 1111, 2222, 3333 |-
user.b@test.com|12345678| 2222             |-
user.c@test.com|12345678|           -      |-
user.d@test.com|12345678|           -      | 1111, 3333

