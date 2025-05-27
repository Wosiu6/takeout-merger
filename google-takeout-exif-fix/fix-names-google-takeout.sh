find . -type f -name "*.json" | while read file; do
#Generate the new name by removing the unwanted strings.
new_name="${file/supplemental-metadata.json/json}"
new_name="${new_name/suppl.json/json}"
new_name="${new_name/suplemen.json/json}"
new_name="${new_name/suplement.json/json}"
new_name="${new_name/suppleme.json/json}"
new_name="${new_name/supple.json/json}"
new_name="${new_name/supplem.json/json}"
new_name="${new_name/supplemen.json/json}"
new_name="${new_name/supplement.json/json}"
new_name="${new_name/supplemental-met.json/json}"
new_name="${new_name/supplemental-.json/json}"
new_name="${new_name/supplemental-m.json/json}"
new_name="${new_name/supplemental-me.json/json}"
new_name="${new_name/supplemental-meta.json/json}"
new_name="${new_name/supplemental-metad.json/json}"
new_name="${new_name/supplemental-metada.json/json}"
new_name="${new_name/supplemental-metadat.json/json}"
new_name="${new_name/supplemental-metadata.json/json}"
new_name="${new_name/supplemental.json/json}"
new_name="${new_name/supplementa.json/json}"
new_name="${new_name/s.json/json}"
new_name="${new_name/su.json/json}"
new_name="${new_name/sup.json/json}"
new_name="${new_name/supp.json/json}"
new_name="${new_name/suppl.json/json}"
new_name="${new_name/supple.json/json}"

# Rename the file only if the name changed.
if [[ "$file" != "$new_name" ]]; then
    mv "$file" "$new_name"
    echo "✅ Renamed: $file -> $new_name"
fi
done

echo "🎉 Process completed in all subfolders."
pause