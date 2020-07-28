﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OneIdentity.DevOps.Attributes;
using OneIdentity.DevOps.Data;
using OneIdentity.DevOps.Data.Spp;
using OneIdentity.DevOps.Logic;
using A2ARetrievableAccount = OneIdentity.DevOps.Data.Spp.A2ARetrievableAccount;
#pragma warning disable 1573

namespace OneIdentity.DevOps.Controllers.V1
{
    /// <summary>
    /// Manage the configuration of the DevOps service and its association with Safeguard for Privileged Passwords.
    /// </summary>
    [ApiController]
    [Route("service/devops/v1/[controller]")]
    public class SafeguardController : ControllerBase
    {
        private readonly Serilog.ILogger _logger;

        /// <summary>
        /// Default constructor for SafeguardController.
        /// </summary>
        public SafeguardController()
        {
            _logger = Serilog.Log.Logger;
        }

        /// <summary>
        /// Get the Safeguard appliance connection information being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service must be associated with a Safeguard for Privileged Passwords appliance before it can be used.
        /// This appliance will be trusted for authentication.  It is also the appliance that will notify the DevOps service
        /// of secret changes so that they can be pushed to the configured plugins.
        /// </remarks>
        /// <response code="200">Success.</response>
        [UnhandledExceptionError]
        [HttpGet]
        public ActionResult<SafeguardConnection> GetSafeguard([FromServices] ISafeguardLogic safeguard)
        {
            var safeguardConnection = safeguard.GetSafeguardConnection();

            return Ok(safeguardConnection);
        }

        /// <summary>
        /// Set the Safeguard appliance connection information for the DevOps service to use.
        /// </summary>
        /// <remarks>
        /// The DevOps service must be associated with a Safeguard for Privileged Passwords appliance before it can be used.
        /// This appliance will be trusted for authentication.  It is also the appliance that will notify the DevOps service
        /// of secret changes so that they can be pushed to the configured plugins.
        /// </remarks>
        /// <param name="safeguardData">Information that represents the Safeguard appliance that should be associated with the DevOps service.</param>
        /// <response code="200">Success.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="401">Invalid authorization token.</response>
        [UnhandledExceptionError]
        [HttpPut]
        public ActionResult<SafeguardConnection> SetSafeguard([FromServices] ISafeguardLogic safeguard,
            [FromBody] SafeguardData safeguardData)
        {
            var token = WellKnownData.GetSppToken(HttpContext);
            var appliance = safeguard.SetSafeguardData(token, safeguardData);

            return Ok(appliance);
        }

        /// <summary>
        /// Delete the Safeguard appliance connection information being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service must be associated with a Safeguard for Privileged Passwords appliance before it can be used.
        /// This appliance will be trusted for authentication.  It is also the appliance that will notify the DevOps service
        /// of secret changes so that they can be pushed to the configured plugins.
        /// 
        /// This endpoint will remove the currently configured association.  It does not clean up any of the DevOps service
        /// related items added to the Safeguard for Privileged Passwords configuration.  Those must be removed manually.
        /// 
        /// It will also remove the DevOps service configuration database and restart the DevOps service.
        /// 
        /// To help prevent unintended Safeguard appliance connection removal, the confirm query param is required and must be set to "yes".
        /// 
        /// (see DELETE /service/devops/{version}/Safeguard/Configuration)
        /// </remarks>
        /// <param name="confirm">This query parameter must be set to "yes" if the caller intends to remove the Safeguard appliance connection.</param>
        /// <response code="204">Success.</response>
        /// <response code="400">Bad Request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpDelete]
        public ActionResult DeleteSafeguard([FromServices] ISafeguardLogic safeguard, [FromQuery] string confirm)
        {
            if (confirm == null || !confirm.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                return BadRequest();

            safeguard.DeleteSafeguardData();

            return NoContent();
        }

        /// <summary>
        /// Get the Safeguard client configuration information being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses client certificate authentication and the A2A service to access Safeguard for Privileged
        /// Passwords to monitor account secret changes and to pull secrets.  The DevOps service also proxies configuration
        /// requests to Safeguard for Privileged Passwords as the currently authenticated administrator user.
        /// </remarks>
        /// <response code="200">Success.</response>
        /// <response code="400">Bad request.</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("Configuration")]
        public ActionResult<ServiceConfiguration> GetDevOpsConfiguration([FromServices] ISafeguardLogic safeguard)
        {
            var serviceConfiguration = safeguard.GetDevOpsConfiguration();

            return Ok(serviceConfiguration);
        }

        /// <summary>
        /// Generate and configure the Safeguard client configuration information for the DevOps service to use.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses client certificate authentication and the A2A service to access Safeguard for Privileged
        /// Passwords to monitor account secret changes and to pull secrets.  The DevOps service also proxies configuration
        /// requests to Safeguard for Privileged Passwords as the currently authenticated administrator user.
        ///
        /// This endpoint will modify configuration stored in Safeguard for Privileged Passwords.  The client certificate that will
        /// be used to create the A2A user in Safeguard for Privileged Passwords can be uploaded as part of the this /Configuration
        /// endpoint or can be uploaded separately in the POST /ClientCertificate endpoint.
        /// 
        /// If the client certificate was already uploaded using the ClientCertificate endpoint, it does not need to be provided
        /// in this operation and the POST body should be empty. ("{}")
        /// 
        /// (see POST /service/devops/{version}/Safeguard/ClientCertificate)
        /// </remarks>
        /// <param name="certFile">If the "Base64CertificateData" parameter contains a base64 encoded (or PEM) A2A client certificate, the certificate
        /// will be installed before the DevOps service completes the configuration. Providing the A2A client certificate in the /Configuration call
        /// replaces the need to install the certificate separately.</param>
        /// <response code="200">Success.</response>
        /// <response code="400">Bad request.</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpPost("Configuration")]
        public ActionResult<ServiceConfiguration> ConfigureSafeguard([FromServices] ISafeguardLogic safeguard, CertificateInfo certFile = null)
        {
            if (certFile?.Base64CertificateData != null)
            {
                safeguard.InstallCertificate(certFile, CertificateType.A2AClient);
            }

            var devOpsConfiguration = safeguard.ConfigureDevOpsService();

            return Ok(devOpsConfiguration);
        }

        /// <summary>
        /// Delete the Safeguard client configuration information being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses client certificate authentication and the A2A service to access Safeguard for Privileged
        /// Passwords to monitor account secret changes and to pull secrets.  The DevOps service also proxies configuration
        /// requests to Safeguard for Privileged Passwords as the currently authenticated administrator user.
        /// 
        /// This endpoint will remove all A2A credential retrievals, the A2A registration and the A2A user from Safeguard for
        /// Privileged Passwords.  It will also remove the DevOps service configuration database and restart the DevOps service.
        /// 
        /// To help prevent unintended Safeguard client configuration removal, the confirm query param is required and must be set to "yes".
        /// </remarks>
        /// <param name="confirm">This query parameter must be set to "yes" if the caller intends to remove the Safeguard client configuration.</param>
        /// <response code="204">No Content.</response>
        /// <response code="400">Bad request.</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpDelete("Configuration")]
        public ActionResult DeleteSafeguardConfiguration([FromServices] ISafeguardLogic safeguard, [FromQuery] string confirm)
        {
            if (confirm == null || !confirm.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                return BadRequest();

            safeguard.DeleteDevOpsConfiguration();

            return NoContent();
        }

        /// <summary>
        /// Logon to the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service trusts Safeguard for Privileged Passwords for administrator authentication.  In order to authenticate
        /// using this endpoint the Authorization header must contain a valid Safeguard API token.  This token can be acquired by
        /// logging into Safeguard using the safeguard-ps command 'Connect-Safeguard -NoSessionVariable' and providing valid login
        /// credentials.  A successful authentication will respond with a sessionKey that should be provided as a cookie for all
        /// subsequent endpoint calls.
        /// </remarks>
        /// <response code="200">Success.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized.</response>
        [SafeguardTokenAuthorization]
        [UnhandledExceptionError]
        [HttpGet("Logon")]
        public ActionResult<SafeguardConnection> GetSafeguardLogon([FromServices] ISafeguardLogic safeguard)
        {
            var safeguardConnection = safeguard.GetSafeguardConnection();
            if (safeguardConnection == null)
                return NotFound("No Safeguard has not been configured");

            return Ok(safeguardConnection);
        }

        /// <summary>
        /// Logoff from the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service trusts Safeguard for Privileged Passwords for authentication.  A successful authentication includes a
        /// sessionKey that should be provided as a cookie for all subsequent endpoint calls.  This endpoint will invalidate that
        /// sessionKey requiring that an administrator re-authenticate.
        /// </remarks>
        /// <response code="200">Success.</response>
        /// <response code="400">Bad Request.</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("Logoff")]
        public ActionResult<SafeguardConnection> GetSafeguardLogoff([FromServices] ISafeguardLogic safeguard)
        {
            var sessionKey = HttpContext.Items["session-key"].ToString();
            AuthorizedCache.Instance.Remove(sessionKey);

            return Ok();
        }

        /// <summary>
        /// Get the A2A client certificate being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses client certificate authentication to access the A2A service in Safeguard for Privileged Passwords.
        /// The most secure way to create this certificate is using a certificate signing request (CSR).
        /// 
        /// (see GET /service/devops/v1/Safeguard/CSR)
        /// </remarks>
        /// <response code="200">Success.</response>
        /// <response code="404">Not found.</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("ClientCertificate")]
        public ActionResult<CertificateInfo> GetClientCertificate([FromServices] ISafeguardLogic safeguard)
        {
            var certificate = safeguard.GetCertificateInfo(CertificateType.A2AClient);
            if (certificate == null)
                return NotFound();

            return Ok(certificate);
        }

        /// <summary>
        /// Upload the A2A client certificate for the DevOps service to use.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses client certificate authentication to access the A2A service in Safeguard for Privileged Passwords.
        /// The most secure way to create this certificate is using a certificate signing request (CSR).
        ///
        /// This endpoint can receive either a PFX formatted certificate that includes the private key and a passphrase for decrypting
        /// that certificate, or it can receive a base64 (or PEM) encoded certificate that was issued based on a generated CSR.
        ///
        /// A client certificate must be uploaded before calling the POST /service/devops/v1/Safeguard/Configure endpoint.
        ///
        /// (see GET /service/devops/v1/Safeguard/CSR)
        /// </remarks>
        /// <param name="certInfo">The certificate info should contain the base64 (or PEM) encoded certificate and pass phrase if the certificate includes a private key.</param>
        /// <response code="200">Success.</response>
        /// <response code="400">Bad request.</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpPost("ClientCertificate")]
        public ActionResult InstallClientCertificate([FromServices] ISafeguardLogic safeguard, CertificateInfo certInfo)
        {
            safeguard.InstallCertificate(certInfo, CertificateType.A2AClient);
            var certificate = safeguard.GetCertificateInfo(CertificateType.A2AClient);

            return Ok(certificate);
        }

        /// <summary>
        /// Delete the A2A client certificate being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses client certificate authentication to access the A2A service in Safeguard for Privileged Passwords.
        /// 
        /// This endpoint removes the current client certificate from the DevOps service.
        /// </remarks>
        /// <response code="204">No Content.</response>
        /// <response code="400">Bad Request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpDelete("ClientCertificate")]
        public ActionResult RemoveClientCertificate([FromServices] ISafeguardLogic safeguard)
        {
            safeguard.RemoveClientCertificate();

            return NoContent();
        }

        /// <summary>
        /// Get the web server certificate being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service use TLS to authenticate itself and to protect its API.  The first time it starts
        /// it generates a self-signed web server certificate.  To ensure secure access this web certificate should
        /// be replaced.  The most secure way to create this certificate is using a certificate signing request (CSR).
        ///
        /// (see GET /service/devops/v1/Safeguard/CSR)
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("WebServerCertificate")]
        public ActionResult<CertificateInfo> GetWebServerCertificate([FromServices] ISafeguardLogic safeguard)
        {
            var certificate = safeguard.GetCertificateInfo(CertificateType.WebSsl);
            if (certificate == null)
                return NotFound();

            return Ok(certificate);
        }

        /// <summary>
        /// Upload the web server certificate for the DevOps service to use.
        /// </summary>
        /// 
        /// <remarks>
        /// The DevOps service use TLS to authenticate itself and to protect its API.  The first time it starts
        /// it generates a self-signed web server certificate.  To ensure secure access this web certificate should
        /// be replaced.  The most secure way to create this certificate is using a certificate signing request (CSR).
        /// 
        /// This endpoint can receive either a PFX formatted certificate that includes the private key and a passphrase for decrypting
        /// that certificate, or it can receive a base64 (or PEM) encoded certificate that was issued based on a generated CSR.
        /// 
        /// The DevOps service will be restarted so the new certificate can be applied.
        /// 
        /// (see GET /service/devops/v1/Safeguard/CSR)
        /// </remarks>
        /// <param name="certInfo">The certificate info should contain the base64 (or PEM) encoded certificate and pass phrase if the certificate includes a private key.</param>
        /// <param name="restart">Restart the DevOps service after certificate installation.</param>
        /// <response code="200">Success</response>
        /// <response code="400">Bad request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpPost("WebServerCertificate")]
        public ActionResult InstallWebServerCertificate([FromServices] ISafeguardLogic safeguard, [FromBody] CertificateInfo certInfo, [FromQuery] bool restart = true)
        {
            safeguard.InstallCertificate(certInfo, CertificateType.WebSsl);
            var certificate = safeguard.GetCertificateInfo(CertificateType.WebSsl);

            if (restart)
                safeguard.RestartService();

            return Ok(certificate);
        }

        /// <summary>
        /// Delete the web server certificate being used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service use TLS to authenticate itself and to protect its API.  The first time it starts
        /// it generates a self-signed web server certificate.  To ensure secure access this web certificate should
        /// be replaced.  The most secure way to create this certificate is using a certificate signing request (CSR).
        ///
        /// This endpoint will remove the current web server certificate and will generate a new self-signed certificate
        /// to take its place.
        /// 
        /// The DevOps service must be restarted before the new self-signed web server certificate will be applied.
        /// </remarks>
        /// <param name="restart">Restart the DevOps service after installing a new default certificate.</param>
        /// <response code="204">No Content</response>
        /// <response code="400">Bad Request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpDelete("WebServerCertificate")]
        public ActionResult RemoveWebServerCertificate([FromServices] ISafeguardLogic safeguard, [FromQuery] bool restart = true)
        {
            safeguard.RemoveWebServerCertificate();

            if (restart)
                safeguard.RestartService();

            return NoContent();
        }

        /// <summary>
        /// Get a CSR that can be signed and the resulting certificate uploaded to the DevOps service.
        /// </summary>
        /// <remarks>
        /// Using a certificate signing request is the most secure method for configuring a web server certificate or
        /// a client certificate in the DevOps service.  This is because the private key never leaves the DevOps service.
        /// 
        /// This endpoint will generate a CSR and return it in PKCS#10 PEM format.  This can be submitted to your own
        /// secure certificate authority (CA) resulting in a signed certificate.  This certificate can be uploaded as a
        /// web server certificate or a client certificate for the DevOps service to use for secure communications.
        /// 
        /// (see POST /service​/devops​/v1​/Safeguard​/ClientCertificate)
        /// (see POST /service/devops/v1/Safeguard/WebServerCertificate)
        /// </remarks>
        /// <param name="size">Size of the certificate</param>
        /// <param name="subjectName">Subject name of the certificate</param>
        /// <param name="sanDns">DNS subject alternative names</param>
        /// <param name="sanIp">IP subject alternative names</param>
        /// <param name="certType">Type of CSR to create.  Types: A2AClient (default), WebSsl</param>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("CSR")]
        public ActionResult<string> GetClientCsr([FromServices] ISafeguardLogic safeguard, [FromQuery] int? size, 
            [FromQuery] string subjectName, [FromQuery] string sanDns, [FromQuery] string sanIp, [FromQuery] string certType = "A2AClient")
        {
            if (!Enum.TryParse(certType, true, out CertificateType cType))
                return BadRequest("Invalid certificate type");

            var csr = safeguard.GetCSR(size, subjectName, sanDns, sanIp, cType);
            return Ok(csr);
        }

        /// <summary>
        /// Get available Safeguard asset accounts that can registered with the DevOps service.
        /// </summary>
        /// <remarks>
        /// This endpoint returns asset accounts from the associated Safeguard for Privileged Passwords appliance that
        /// can be registered with the DevOps service.  This registration occurs by adding these asset accounts as
        /// retrievable accounts to the A2A registration associated with this DevOps service.  Adding and removing
        /// asset account registrations should be done using the DevOps service.
        ///
        /// (see GET /service​/devops​/v1​/Safeguard​/A2ARegistration​/RetrievableAccounts)
        /// (see POST /service​/devops​/v1​/Safeguard​/A2ARegistration​/RetrievableAccounts)
        /// </remarks>
        /// <param name="filter">Filter results. Available operators: eq, ne, gt, ge, lt, le, and, or, not, contains, ieq, icontains, in [ {item1}, {item2}, etc], (). Use \ to escape quotes in strings.</param>
        /// <param name="page">Which page (starting with 0) of data to return</param>
        /// <param name="limit">The size of a page of data</param>
        /// <param name="orderby">List of property names (comma-separated) to sort entities by. Prepend properties with - for descending.</param>
        /// <param name="q">Search all string fields for the specified value</param>
        /// <response code="200">Success</response>
        /// <response code="400">Bad Request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("AvailableAccounts")]
        public ActionResult<IEnumerable<SppAccount>> GetAvailableAccounts([FromServices] ISafeguardLogic safeguard, [FromQuery] string filter = null, 
            [FromQuery] int? page = null, [FromQuery] int? limit = null, [FromQuery] string orderby = null, [FromQuery] string q = null)
        {
            var availableAccounts = safeguard.GetAvailableAccounts(filter, page, limit, orderby, q);

            return Ok(availableAccounts);
        }

        /// <summary>
        /// Get the A2A registration used by the DevOps service.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses the Safeguard for Privileged Passwords A2A service to access
        /// to monitor account secret changes and to pull secrets. The DevOps service create a special A2A registration
        /// that can have registered accounts.  Each account that is registered with this A2A registration, will be monitored
        /// by the DevOps service.
        /// </remarks>
        /// <param name="registrationType">Type of registration.  Types: Account (default), Vault</param>
        /// <response code="200">Success</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not found</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("A2ARegistration")]
        public ActionResult<A2ARegistration> GetA2ARegistration([FromServices] ISafeguardLogic safeguard, [FromQuery] string registrationType = "Account")
        {
            if (!Enum.TryParse(registrationType, true, out A2ARegistrationType rType))
                return BadRequest("Invalid registration type");

            var registration = safeguard.GetA2ARegistration(rType);
            if (registration == null)
                return NotFound();

            return Ok(registration);
        }

        /// <summary>
        /// Get accounts registered with the DevOps service A2A registration.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses the Safeguard for Privileged Passwords A2A service to access
        /// to monitor account secret changes and to pull secrets. The DevOps service create a special A2A registration
        /// that can have registered accounts.  Each account that is registered with this A2A registration, will be monitored
        /// by the DevOps service.
        ///
        /// This endpoint gets a list of accounts that have been registred with the DevOps service A2A registration.
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("A2ARegistration/RetrievableAccounts")]
        public ActionResult<IEnumerable<A2ARetrievableAccount>> GetRetrievableAccounts([FromServices] ISafeguardLogic safeguard)
        {
            var retrievableAccounts = safeguard.GetA2ARetrievableAccounts(A2ARegistrationType.Account);

            return Ok(retrievableAccounts);
        }

        /// <summary>
        /// Register accounts with the DevOps service A2A registration.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses the Safeguard for Privileged Passwords A2A service to access
        /// to monitor account secret changes and to pull secrets. The DevOps service create a special A2A registration
        /// that can have registered accounts.  Each account that is registered with this A2A registration, will be monitored
        /// by the DevOps service.
        ///
        /// This endpoint registers one or more accounts with the DevOps service A2A registration.
        /// </remarks>
        /// <param name="accounts">List of accounts to add to the DevOps service A2A registration</param>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpPut("A2ARegistration/RetrievableAccounts")]
        public ActionResult<IEnumerable<A2ARetrievableAccount>> AddRetrievableAccounts([FromServices] ISafeguardLogic safeguard, IEnumerable<SppAccount> accounts)
        {
            var retrievableAccounts = safeguard.AddA2ARetrievableAccounts(accounts, A2ARegistrationType.Account);

            return Ok(retrievableAccounts);
        }

        /// <summary>
        /// Unregister accounts with the DevOps service A2A registration.
        /// </summary>
        /// <remarks>
        /// The DevOps service uses the Safeguard for Privileged Passwords A2A service to access
        /// to monitor account secret changes and to pull secrets. The DevOps service create a special A2A registration
        /// that can have registered accounts.  Each account that is registered with this A2A registration, will be monitored
        /// by the DevOps service.
        ///
        /// This endpoint unregisters one or more accounts with the DevOps service A2A registration.
        /// </remarks>
        /// <param name="accounts">List of accounts to remove from the DevOps service A2A registration</param>
        /// <response code="204">Success</response>
        /// <response code="400">Bad Request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpDelete("A2ARegistration/RetrievableAccounts")]
        public ActionResult RemoveRetrievableAccounts([FromServices] ISafeguardLogic safeguard, IEnumerable<A2ARetrievableAccount> accounts)
        {
            safeguard.RemoveA2ARetrievableAccounts(accounts, A2ARegistrationType.Account);

            return NoContent();
        }

        /// <summary>
        /// Restart the DevOps service.
        /// </summary>
        /// <remarks>
        /// Some DevOps service operations require that the service is restarted.  Some of these operations include replacing or regenerating
        /// the DevOps service web SSL certificate and updating third party vault plugins.
        /// </remarks>
        /// <response code="204">Success</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpPost("Restart")]
        public ActionResult RestartService([FromServices] ISafeguardLogic safeguard)
        {
            safeguard.RestartService();

            return NoContent();
        }

        /// <summary>
        /// Get a list of the trusted certificates that the DevOps service uses to validate an SSL connection to a Safeguard.
        /// </summary>
        /// <remarks>
        /// Once the DevOps service has been associated with a Safeguard for Privileged Passwords appliance, a trusted connection should be
        /// established.  Establishing a trusted connection requires that trusted certificates be added to the service.
        ///
        /// This endpoint lists all of the certificates that have been uploaded and trusted by the DevOps service.
        ///
        /// (See PUT /service/devops/{version}/Safeguard/Configuration - ignoreSsl parameter)
        /// </remarks>
        /// <response code="200">Success</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("TrustedCertificates")]
        public ActionResult<IEnumerable<CertificateInfo>> GetTrustedCertificates([FromServices] ISafeguardLogic safeguard)
        {
            var trustedCertificates = safeguard.GetTrustedCertificates();

            return Ok(trustedCertificates);
        }

        /// <summary>
        /// Get a trusted certificate that matches the specified thumbprint.
        /// </summary>
        /// <remarks>
        /// Once the DevOps service has been associated with a Safeguard for Privileged Passwords appliance, a trusted connection should be
        /// established.  Establishing a trusted connection requires that trusted certificates be added to the service.
        ///
        /// This endpoint gets a specific trusted certificate that matches the specified thumbprint.
        /// </remarks>
        /// <param name="thumbprint">Trusted certificate thumbprint.</param>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpGet("TrustedCertificates/{thumbprint}")]
        public ActionResult<CertificateInfo> GetTrustedCertificate([FromServices] ISafeguardLogic safeguard, [FromRoute] string thumbprint)
        {
            var trustedCertificate = safeguard.GetTrustedCertificate(thumbprint);

            return Ok(trustedCertificate);
        }

        /// <summary>
        /// Add a trusted certificate to the DevOps service.
        /// </summary>
        /// <remarks>
        /// Once the DevOps service has been associated with a Safeguard for Privileged Passwords appliance, a trusted connection should be
        /// established.  Establishing a trusted connection requires that trusted certificates be added to the service.
        ///
        /// This endpoint adds a certificate as a trusted certificate in the DevOps service. An optional "importFromSafeguard" parameter will
        /// allow the DevOps service to import all of the trusted certificates directly from the associated Safeguard for Privileged Passwords appliance.
        /// </remarks>
        /// <param name="certificate">Trusted certificate to add.</param>
        /// <param name="importFromSafeguard">Import all of the trusted certificates from the associated Safeguard appliance. If this parameter is true then the body will be ignored.</param>
        /// <response code="200">Success</response>
        /// <response code="400">Bad request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpPost("TrustedCertificates")]
        public ActionResult<IEnumerable<CertificateInfo>> AddTrustedCertificate([FromServices] ISafeguardLogic safeguard, [FromBody] CertificateInfo certificate, [FromQuery] bool importFromSafeguard)
        {
            IEnumerable<CertificateInfo> trustedCertificates;

            if (importFromSafeguard)
            {
                trustedCertificates = safeguard.ImportTrustedCertificates();
            }
            else
            {
                var trustedCertificate = safeguard.AddTrustedCertificate(certificate);
                trustedCertificates = new List<CertificateInfo>() {trustedCertificate};
            }

            return Ok(trustedCertificates);
        }

        /// <summary>
        /// Delete a trusted certificate from the DevOps service.
        /// </summary>
        /// <remarks>
        /// Once the DevOps service has been associated with a Safeguard for Privileged Passwords appliance, a trusted connection should be
        /// established.  Establishing a trusted connection requires that trusted certificates be added to the service.
        ///
        /// This endpoint removes the certificate that is identified by the thumbprint, as a trusted certificate in the DevOps service.
        /// </remarks>
        /// <param name="thumbprint">Certificate thumbprint.</param>
        /// <response code="204">Success</response>
        /// <response code="400">Bad Request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpDelete("TrustedCertificates/{thumbprint}")]
        public ActionResult DeleteTrustedCertificate([FromServices] ISafeguardLogic safeguard, [FromRoute] string thumbprint)
        {
            safeguard.DeleteTrustedCertificate(thumbprint);

            return NoContent();
        }

        /// <summary>
        /// Delete all of the trusted certificate from the DevOps service.
        /// </summary>
        /// <remarks>
        /// Once the DevOps service has been associated with a Safeguard for Privileged Passwords appliance, a trusted connection should be
        /// established.  Establishing a trusted connection requires that trusted certificates be added to the service.
        ///
        /// This endpoint removes all of the trusted certificates from the DevOps service.
        /// 
        /// To help prevent unintended trusted certificate removal, the confirm query param is required and must be set to "yes".
        /// </remarks>
        /// <param name="confirm">This query parameter must be set to "yes" if the caller intends to remove all of the trusted certificates.</param>
        /// <response code="204">Success</response>
        /// <response code="400">Bad Request</response>
        [SafeguardSessionKeyAuthorization]
        [UnhandledExceptionError]
        [HttpDelete("TrustedCertificates")]
        public ActionResult DeleteAllTrustedCertificate([FromServices] ISafeguardLogic safeguard, [FromQuery] string confirm)
        {
            if (confirm == null || !confirm.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                return BadRequest();

            safeguard.DeleteAllTrustedCertificates();

            return NoContent();
        }

    }
}
