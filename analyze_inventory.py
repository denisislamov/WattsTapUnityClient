import os, glob

base = "/Users/denisislamov/WorkProjects/Unity/WattsTap/WattsTapUnityClient"
items_dir = os.path.join(base, "Assets/WattsTap/Configs/Catalog/Items")

inv_guids = [
    "c69399ca9d474482493a88944301c9bf",
    "997aa2a779b4c4ae5a46db282c9e2c4b",
    "3e2747d3b12dd4989a544a31e1b25234",
    "c558f2f76c0f24fd38410327fbb1c181",
    "df3d5e4e8149248fe99ddc53686a18b0",
    "9bde6e7eea290463eb1d031471e4c7d3",
    "d84cbc5ed8dcc4257a2f9949b9eb7ebb",
    "a0ca8b9921fbd4183b2a439abe0a33ea",
    "1ffef20ed57d24ec4a3c93f0c6b1fc74",
    "8d22cf338e14746418e8caa9580d74cd",
    "b65b2baf39380484c91650d0cbe6294b",
    "1afb8f9811f424032b3f769b1ab838ef",
    "7fa8fde2f378e4714a0aad970e760cb9",
    "feb4860caf9d0458daf25484a13d85f0",
]

cat_file = os.path.join(base, "Assets/WattsTap/Configs/Catalog/CatalogConfig.asset")
cat_guids = []
with open(cat_file) as f:
    for line in f:
        if "guid:" in line and "m_Script" not in line:
            parts = line.split("guid: ")
            if len(parts) > 1:
                g = parts[1].split(",")[0].strip()
                cat_guids.append(g)

guid_to_file = {}
for meta_path in glob.glob(os.path.join(items_dir, "*.meta")):
    with open(meta_path) as f:
        for line in f:
            if line.startswith("guid:"):
                g = line.split("guid:")[1].strip()
                asset_name = os.path.basename(meta_path).replace(".meta", "")
                guid_to_file[g] = asset_name
                break

print("=== InventoryStartConfig items ===")
for g in inv_guids:
    name = guid_to_file.get(g, "UNKNOWN (not in Items/)")
    in_catalog = g in cat_guids
    print("  {} -> {} (in catalog: {})".format(g, name, in_catalog))

print("\nTotal Catalog items: {}".format(len(cat_guids)))
print("Total Inventory start items: {}".format(len(inv_guids)))

not_in_catalog = [g for g in inv_guids if g not in cat_guids]
print("\nInventory items NOT in catalog: {}".format(len(not_in_catalog)))
for g in not_in_catalog:
    name = guid_to_file.get(g, "UNKNOWN")
    print("  {} -> {}".format(g, name))

