WCF Assemblies are organised to try and separate:
	* code used by Generated Libraries.
	* code used purely on WCF Service side.
	* code used purely on WCF Client side.
	* code shared between combinations of the above.

Below Generated means code created by a code generator, and Manual means code written by a programmer.

Udbus.WCF
WCF Manual Client code, and code common to WCF Manual Client and WCF Manual Service.

Udbus.WCF.Service
WCF Manual Service code.

Udbus.WCF.Dbus
Code common to Generated Client and Service code.

Udbus.WCF.Dbus.Common
Code for Generated Service code to prevent circular Assembly dependencies.

Udbus.WCF.Dbus.Details
Code for Generated Service code.

Compiling generated code will need the following:
DbusInterfaces - 
	Udbus.Containers.
DbusServices - 
	DbusInterfaces
	Udbus.Containers
	Udbus.Core
	Udbus.Serialization
	Udbus.Types
DbusClients (currently unused) - 
	DbusInterfaces
	Udbus.Containers
	UdbusCSharpLib
	UdbusParsingLib
	UdbusCSharpGenerated.
DbusWCFContracts - 
	DbusInterfaces
	Udbus.Containers
	Udbus.WCF
	Udbus.WCF.Dbus.
DbusWCFServices - 
	DbusInterfaces
	DbusWCFContracts
	Udbus.Containers.
DbusWCFHosts - 
	DbusInterfaces
	DbusServices
	DbusWCFContracts
	DbusWCFServices
	Udbus.Core
	Udbus.Serialization
	Udbus.WCF.Dbus.Common
	Udbus.WCF.Dbus.Details
	Udbus.WCF.Service.

A WCF client application
	DbusInterfaces
	DbusWCFContracts
	Udbus.Containers
	Udbus.Types
