find . -type f -name "*.json" | while read file; do
  new_name=$(echo "$file" | sed -E 's/(supplemental-metadata|supplemental-metadat|supplemental-metada|supplemental-metad|supplemental-meta|supplemental-met|supplemental-me|supplemental-m|supplemental-|supplemental|supplementa|supplemen|suppleme|supplem|supple|suppl|supp|sup|su|s)\.json$/\.json/')

  if [[ "$file" != "$new_name" ]]; then
    mv "$file" "$new_name"
    echo "✅ Renamed: $file -> $new_name"
  fi
done

echo "🎉 Process completed in all subfolders."
