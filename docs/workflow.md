# Process primary input datasets
Many primary data inputs are needed for the base NSI data, however, other processes may have less primary data to work through so these steps may not be possible. In those cases skipping this to get to a later step may need to be an external workflow to create the next layer input data.

## create consolidated footprint dataset
Multiple input footprint data sources may exist, the input footprint datasets are consolidated to the Footprint dataset that represents the best footprint for each structure across the dataset.

## distribute population
The census data and the LEHD data work together to generate a base population dataset. This is amended ultimately with the fortified parcel data that has population characteristics on the per parcel dataset leveraging things like schools, hospitals and prisons.

## fortify parcel data
The input parcel data is joined with auxiliary datasets like schools and prisons and hospitals to assign the appropriate information into the parcel dataset including an optional point geometry representing the point location and various population and type characteristics.

# process secondary input datasets
3. process final data
