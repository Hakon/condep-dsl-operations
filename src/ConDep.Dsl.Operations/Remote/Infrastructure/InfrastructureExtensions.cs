﻿using System;
using System.Security.AccessControl;
using System.Web.Configuration;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Operations.Application.Deployment.WindowsService;
using ConDep.Dsl.Operations.Builders;
using ConDep.Dsl.Operations.Infrastructure.IIS;
using ConDep.Dsl.Operations.Infrastructure.IIS.AppPool;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebApp;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebSite;
using ConDep.Dsl.Operations.Infrastructure.Windows;
using ConDep.Dsl.Operations.Remote.Infrastructure.IIS;
using ConDep.Dsl.Operations.Remote.Infrastructure.IIS.AppPool;
using ConDep.Dsl.Operations.Remote.Infrastructure.IIS.MachineKey;
using ConDep.Dsl.Operations.Remote.Infrastructure.IIS.WebApp;
using ConDep.Dsl.Operations.Remote.Infrastructure.IIS.WebSite;
using ConDep.Dsl.Operations.Remote.Infrastructure.Windows;
using ConDep.Dsl.Operations.Remote.Infrastructure.Windows.Acl;
using ConDep.Dsl.Operations.Remote.Infrastructure.Windows.EnvironmentVariable;
using ConDep.Dsl.Operations.Remote.Infrastructure.Windows.Registry;
using ConDep.Dsl.Operations.Remote.Infrastructure.Windows.UserAdmin;
using ConDep.Dsl.Operations.Remote.Infrastructure.Windows.WindowsService;
using Microsoft.Win32;

namespace ConDep.Dsl
{
    public static class InfrastructureExtensions
    {
        /// <summary>
        /// Installs and configures IIS with provided options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IIS(this IOfferRemoteConfiguration infra, Action<IisInfrastructureOptions> options)
        {
            var op = new IisInfrastructureOperation();
            options(new IisInfrastructureOptions(op));

            OperationExecutor.Execute((RemoteBuilder) infra, op);
            return infra;
        }

        /// <summary>
        /// Installs IIS
        /// </summary>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IIS(this IOfferRemoteConfiguration infra)
        {
            var op = new IisInfrastructureOperation();
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Offer common Windows operations
        /// </summary>
        /// <returns></returns>
        public static IOfferRemoteConfiguration Windows(this IOfferRemoteConfiguration infra, Action<WindowsInfrastructureOptions> options)
        {
            var op = new WindowsFeatureInfrastructureOperation();
            options(new WindowsInfrastructureOptions(op));
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Creates a new Web Site in IIS if not exist. If exist, will delete and then create new.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IISWebSite(this IOfferRemoteConfiguration infra, string name, int id)
        {
            var op = new IisWebSiteOperation(name, id);
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Creates a new Web Site in IIS if not exist. If exist, will delete and then create new with provided options.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IISWebSite(this IOfferRemoteConfiguration infra, string name, int id, Action<IOfferIisWebSiteOptions> options)
        {
            var opt = new IisWebSiteOptions();
            options(opt);
            var op = new IisWebSiteOperation(name, id, opt);
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Will create a new Application Pool in IIS.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IISAppPool(this IOfferRemoteConfiguration infra, string name)
        {
            var op = new IisAppPoolOperation(name);
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Will create a new Application Pool in IIS with provided options.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IISAppPool(this IOfferRemoteConfiguration infra, string name, Action<IOfferIisAppPoolOptions> options)
        {
            var opt = new IisAppPoolOptions();
            options(opt);
            var op = new IisAppPoolOperation(name, opt.Values);
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Will create a new Web Application in IIS under the given Web Site.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="webSite"></param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IISWebApp(this IOfferRemoteConfiguration infra, string name, string webSite)
        {
            var op = new IisWebAppOperation(name, webSite);
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Will create a new Web Application in IIS under the given Web Site, with the provided options.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="webSite"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IISWebApp(this IOfferRemoteConfiguration infra, string name, string webSite, Action<IOfferIisWebAppOptions> options)
        {
            var builder = new IisWebAppOptions(name);
            options(builder);

            var op = new IisWebAppOperation(name, webSite, builder.Values);
            OperationExecutor.Execute((RemoteBuilder)infra, op);
            return infra;
        }

        /// <summary>
        /// Provide operations for installing SSL certificates.
        /// </summary>
        public static IOfferSslInfrastructure SslCertificate(this IOfferRemoteConfiguration infra)
        {
            var builder = infra as RemoteConfigurationBuilder;
            return new SslInfrastructureBuilder(infra, builder.Server, builder.Settings, builder.Token);
        }

        /// <summary>
        /// Disables User Account Control. The operation is idempotent and will trigger a restart, but only if UAC not is already disabled. 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="enabled">Specify if you want UAC enabled or not. E.g. setting this to false will disable UAC.</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration UserAccountControl(this IOfferRemoteConfiguration configuration, bool enabled)
        {
            var operation = new UserAccountControlOperation(enabled);
            OperationExecutor.Execute((RemoteBuilder)configuration, operation);
            return configuration;
        }

        /// <summary>
        /// Gives access to Windows Registry operations 
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="reg">Windows Registry operations</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration WindowsRegistry(this IOfferRemoteConfiguration conf, Action<IOfferWindowsRegistryOperations> reg)
        {
            var confBuilder = conf as RemoteConfigurationBuilder;
            var builder = new WindowsRegistryBuilder(conf, confBuilder.Server, confBuilder.Settings, confBuilder.Token);
            reg(builder);
            return conf;
        }

        /// <summary>
        /// Creates a Windows Registry key with default value and optional values and sub keys.
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="root">The Windows Registry hive to use. See <see cref="WindowsRegistryRoot"/> for available options. Example: WindowsRegistryRoot.HKEY_LOCAL_MACHINE</param>
        /// <param name="key">Name of the key to create. Example: SOFTWARE\ConDep</param>
        /// <param name="defaultValue">The default value of the key</param>
        /// <param name="options">Additional options for setting Windows Registry values and sub keys.</param>
        /// <returns></returns>
        public static IOfferWindowsRegistryOperations CreateKey(this IOfferWindowsRegistryOperations reg, WindowsRegistryRoot root, string key, string defaultValue, Action<IOfferWindowsRegistryOptions> options = null)
        {
            var optBuilder = new WindowsRegistryOptionsBuilder();

            if (options != null)
            {
                options(optBuilder);
            }

            var valuesBuilder = optBuilder.Values as WindowsRegistryValueBuilder;
            var keysBuilder = optBuilder.SubKeys as WindowsRegistrySubKeyBuilder;

            var op = new CreateWindowsRegistryKeyOperation(root, key, defaultValue, valuesBuilder.Values, keysBuilder.Keys);
            var regBuilder = reg as WindowsRegistryBuilder;
            OperationExecutor.Execute((RemoteBuilder)reg, op);
            return reg;
        }

        /// <summary>
        /// Creates a Windows Registry key and optional values and sub keys.
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="root">The Windows Registry hive to use. See <see cref="WindowsRegistryRoot"/> for available options. Example: WindowsRegistryRoot.HKEY_LOCAL_MACHINE</param>
        /// <param name="key">Name of the key to create. Example: SOFTWARE\ConDep</param>
        /// <param name="options">Additional options for setting Windows Registry values and sub keys.</param>
        /// <returns></returns>
        public static IOfferWindowsRegistryOperations CreateKey(this IOfferWindowsRegistryOperations reg, WindowsRegistryRoot root, string key, Action<IOfferWindowsRegistryOptions> options = null)
        {
            return CreateKey(reg, root, key, "", options);
        }

        /// <summary>
        /// Creates or updates a Windows Registry value.
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="root">The Windows Registry hive to use. See <see cref="WindowsRegistryRoot"/> for available options. Example: WindowsRegistryRoot.HKEY_LOCAL_MACHINE</param>
        /// <param name="key">Name of the key containing the value you want to create or update. Example: SOFTWARE\ConDep</param>
        /// <param name="valueName">Name of the registry value</param>
        /// <param name="valueData">The data value you want to set</param>
        /// <param name="valueKind">The data type to use when storing values in the registry</param>
        /// <returns></returns>
        public static IOfferWindowsRegistryOperations SetValue(this IOfferWindowsRegistryOperations reg, WindowsRegistryRoot root, string key, string valueName, string valueData, RegistryValueKind valueKind)
        {
            var op = new SetWindowsRegistryValueOperation(root, key, valueName, valueData, valueKind);
            var regBuilder = reg as WindowsRegistryBuilder;
            OperationExecutor.Execute((RemoteBuilder)reg, op);
            return reg;
        }

        /// <summary>
        /// Deletes a Windows Registry key.
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="root">The Windows Registry hive to use. See <see cref="WindowsRegistryRoot"/> for available options. Example: WindowsRegistryRoot.HKEY_LOCAL_MACHINE</param>
        /// <param name="key">Name of the key you want to delete. Example: SOFTWARE\ConDep</param>
        /// <returns></returns>
        public static IOfferWindowsRegistryOperations DeleteKey(this IOfferWindowsRegistryOperations reg, WindowsRegistryRoot root, string key)
        {
            var op = new DeleteWindowsRegistryKeyOperation(root, key);
            var regBuilder = reg as WindowsRegistryBuilder;
            OperationExecutor.Execute((RemoteBuilder)reg, op);
            return reg;
        }

        /// <summary>
        /// Deletes a value in Windows Registry.
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="root">The Windows Registry hive to use. See <see cref="WindowsRegistryRoot"/> for available options. Example: WindowsRegistryRoot.HKEY_LOCAL_MACHINE</param>
        /// <param name="key">Name of the key where the value you want to delete exists. Example: SOFTWARE\ConDep</param>
        /// <param name="valueName">Name of the value you want to delete.</param>
        /// <returns></returns>
        public static IOfferWindowsRegistryOperations DeleteValue(this IOfferWindowsRegistryOperations reg, WindowsRegistryRoot root, string key, string valueName)
        {
            var op = new DeleteWindowsRegistryValueOperation(root, key, valueName);
            var regBuilder = reg as WindowsRegistryBuilder;
            OperationExecutor.Execute((RemoteBuilder)reg, op);
            return reg;
        }

        /// <summary>
        /// Creates environment variable if not exists. Overwrites the variable if exists.
        /// </summary>
        /// <param name="configure"></param>
        /// <param name="name">Variable name </param>
        /// <param name="value">Variable value</param>
        /// <param name="target">Variable target</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration EnvironmentVariable(this IOfferRemoteConfiguration configure, string name, string value, EnvironmentVariableTarget target)
        {
            var operation = new EnvironmentVariableOperation(name, value, target);
            OperationExecutor.Execute((RemoteBuilder)configure, operation);
            return configure;
        }

        /// <summary>
        /// Sets the IIS machine key. Configures algorithms and keys to use for encryption, 
        /// decryption, and validation of forms-authentication data and view-state data, and 
        /// for out-of-process session state identification.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="validationKey">Specifies the key used to validate encrypted data</param>
        /// <param name="decryptionKey">Specifies the key that is used to encrypt and decrypt data or the process by which the key is generated</param>
        /// <param name="validation">Specifies the type of encryption that is used to validate data</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration IisMachineKey(this IOfferRemoteConfiguration configuration, string validationKey, string decryptionKey, MachineKeyValidation validation)
        {
            var operation = new SetIisMachineKeyOperation(validationKey, decryptionKey, validation);
            OperationExecutor.Execute((RemoteBuilder)configuration, operation);
            return configuration;
        }

        /// <summary>
        /// Sets ACL (Access Control Lists) on files or folders. Like chmod on Linux.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="user">The user account that will get access</param>
        /// <param name="fileOrFolder">The file or folder to configure ACL for</param>
        /// <param name="accessRights">The access rights to allow</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration Acl(this IOfferRemoteConfiguration configuration, string user, string fileOrFolder, FileSystemRights accessRights)
        {
            var op = new AclOperation(user, fileOrFolder, accessRights, new AclOptions.AclOptionsValues());
            OperationExecutor.Execute((RemoteBuilder)configuration, op);
            return configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="user">The user account that will get access</param>
        /// <param name="fileOrFolder">The file or folder to configure ACL for</param>
        /// <param name="accessRights">The access rights to allow or deny</param>
        /// <param name="options">Additional ACL options</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration Acl(this IOfferRemoteConfiguration configuration, string user, string fileOrFolder, FileSystemRights accessRights, Action<IOfferAclOptions> options)
        {
            var opt = new AclOptions();
            if (options != null)
            {
                options(opt);
            }
            var op = new AclOperation(user, fileOrFolder, accessRights, opt.Values);
            OperationExecutor.Execute((RemoteBuilder)configuration, op);
            return configuration;
        }

        /// <summary>
        /// Will configure provided Windows Service on remote server.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceName">Name of the Windows Service</param>
        /// <param name="serviceDirPath">Path to the directory where the Windows Service is located</param>
        /// <param name="relativeExePath">The relative location (to destDir) of the executable (.exe) for which the Windows Service will execute</param>
        /// <param name="displayName">The display name of the Windows Service as will be displayed in Windows Service Manager</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration WindowsService(this IOfferRemoteConfiguration configuration, string serviceName, string displayName, string serviceDirPath, string relativeExePath)
        {
            return WindowsService(configuration, serviceName, displayName, serviceDirPath, relativeExePath, null);
        }

        /// <summary>
        /// Will configure provided Windows Service on remote server with provided options.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceName">Name of the Windows Service</param>
        /// <param name="serviceDirPath">Path to the directory where the Windows Service is located</param>
        /// <param name="relativeExePath">The relative location (from the Windows Service root directory) of the executable (.exe) for which the Windows Service will execute</param>
        /// <param name="displayName">The display name of the Windows Service as will be displayed in Windows Service Manager</param>
        /// <param name="options">Additional options for the Windows Service</param>
        /// <returns></returns>
        public static IOfferRemoteConfiguration WindowsService(this IOfferRemoteConfiguration configuration, string serviceName, string displayName, string serviceDirPath, string relativeExePath, Action<IOfferWindowsServiceOptions> options)
        {
            var winServiceOptions = new WindowsServiceOptions();
            if (options != null)
            {
                options(winServiceOptions);
            }

            var winServiceOperation = new ConfigureWindowsServiceOperation(serviceName, displayName, serviceDirPath, relativeExePath, winServiceOptions.Values);
            OperationExecutor.Execute((RemoteBuilder)configuration, winServiceOperation);
            return configuration;
        }
    }
}