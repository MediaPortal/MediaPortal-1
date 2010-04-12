/*
 * Helper class for retrieving namespace providers from an external source (i.e. the config file)
 * Copyright (C) 2004 Tom McMillen
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 */

namespace Gentle.Framework
{
	internal class NamespaceProvider
	{
		public readonly string NamespaceMatch;
		public readonly string ProviderName;
		public readonly string ConnectionString;
		public readonly string Schema;

		public NamespaceProvider( string namespaceMatch, string providerName, string connectionString, string schema )
		{
			NamespaceMatch = namespaceMatch;
			ProviderName = providerName;
			ConnectionString = connectionString;
			Schema = schema;
		}
	}
}