namespace TakeoutMerger.Tags
{
    public static class ExifTag
    {
        // Main IFD Tags
        public const int IMAGE_DESCRIPTION = 0x010E;
        public const int MAKE = 0x010F;
        public const int MODEL = 0x0110;
        public const int ORIENTATION = 0x0112;
        public const int X_RESOLUTION = 0x011A;
        public const int Y_RESOLUTION = 0x011B;
        public const int RESOLUTION_UNIT = 0x0128;
        public const int SOFTWARE = 0x0131;
        public const int DATE_TIME = 0x0132;
        public const int WHITE_POINT = 0x013E;
        public const int PRIMARY_CHROMATICITIES = 0x013F;
        public const int YCBCR_COEFFICIENTS = 0x0211;
        public const int YCBCR_POSITIONING = 0x0213;
        public const int REFERENCE_BLACK_WHITE = 0x0214;
        public const int COPYRIGHT = 0x8298;
        public const int EXIF_OFFSET = 0x8769;

        // Exif SubIFD Tags
        public const int EXPOSURE_TIME = 0x829A;
        public const int F_NUMBER = 0x829D;
        public const int EXPOSURE_PROGRAM = 0x8822;
        public const int ISO_SPEED_RATINGS = 0x8827;
        public const int EXIF_VERSION = 0x9000;
        public const int DATE_TIME_ORIGINAL = 0x9003;
        public const int DATE_TIME_DIGITIZED = 0x9004;
        public const int COMPONENT_CONFIGURATION = 0x9101;
        public const int COMPRESSED_BITS_PER_PIXEL = 0x9102;
        public const int SHUTTER_SPEED_VALUE = 0x9201;
        public const int APERTURE_VALUE = 0x9202;
        public const int BRIGHTNESS_VALUE = 0x9203;
        public const int EXPOSURE_BIAS_VALUE = 0x9204;
        public const int MAX_APERTURE_VALUE = 0x9205;
        public const int SUBJECT_DISTANCE = 0x9206;
        public const int METERING_MODE = 0x9207;
        public const int LIGHT_SOURCE = 0x9208;
        public const int FLASH = 0x9209;
        public const int FOCAL_LENGTH = 0x920A;
        public const int MAKER_NOTE = 0x927C;
        public const int USER_COMMENT = 0x9286;
        public const int FLASHPIX_VERSION = 0xA000;
        public const int COLOR_SPACE = 0xA001;
        public const int EXIF_IMAGE_WIDTH = 0xA002;
        public const int EXIF_IMAGE_HEIGHT = 0xA003;
        public const int RELATED_SOUND_FILE = 0xA004;
        public const int EXIF_INTEROPERABILITY_OFFSET = 0xA005;
        public const int FOCAL_PLANE_X_RESOLUTION = 0xA20E;
        public const int FOCAL_PLANE_Y_RESOLUTION = 0xA20F;
        public const int FOCAL_PLANE_RESOLUTION_UNIT = 0xA210;
        public const int SENSING_METHOD = 0xA217;
        public const int FILE_SOURCE = 0xA300;
        public const int SCENE_TYPE = 0xA301;

        // IFD1 (Thumbnail) Tags
        public const int THUMBNAIL_IMAGE_WIDTH = 0x0100;
        public const int THUMBNAIL_IMAGE_LENGTH = 0x0101;
        public const int THUMBNAIL_BITS_PER_SAMPLE = 0x0102;
        public const int THUMBNAIL_COMPRESSION = 0x0103;
        public const int THUMBNAIL_PHOTOMETRIC_INTERPRETATION = 0x0106;
        public const int THUMBNAIL_STRIP_OFFSETS = 0x0111;
        public const int THUMBNAIL_SAMPLES_PER_PIXEL = 0x0115;
        public const int THUMBNAIL_ROWS_PER_STRIP = 0x0116;
        public const int THUMBNAIL_STRIP_BYTE_COUNTS = 0x0117;
        public const int THUMBNAIL_PLANAR_CONFIGURATION = 0x011C;
        public const int THUMBNAIL_JPEG_IF_OFFSET = 0x0201;
        public const int THUMBNAIL_JPEG_IF_BYTE_COUNT = 0x0202;
        public const int THUMBNAIL_YCBCR_SUB_SAMPLING = 0x0212;

        // Miscellaneous Tags
        public const int NEW_SUBFILE_TYPE = 0x00FE;
        public const int SUBFILE_TYPE = 0x00FF;
        public const int TRANSFER_FUNCTION = 0x012D;
        public const int ARTIST = 0x013B;
        public const int PREDICTOR = 0x013D;
        public const int TILE_WIDTH = 0x0142;
        public const int TILE_LENGTH = 0x0143;
        public const int TILE_OFFSETS = 0x0144;
        public const int TILE_BYTE_COUNTS = 0x0145;
        public const int SUB_IFDS = 0x014A;
        public const int JPEG_TABLES = 0x015B;
        public const int CFA_REPEAT_PATTERN_DIM = 0x828D;
        public const int CFA_PATTERN = 0x828E;
        public const int BATTERY_LEVEL = 0x828F;
        public const int IPTC_NAA = 0x83BB;
        public const int INTER_COLOR_PROFILE = 0x8773;
        public const int SPECTRAL_SENSITIVITY = 0x8824;
        public const int GPS_INFO = 0x8825;
        public const int OECF = 0x8828;
        public const int INTERLACE = 0x8829;
        public const int TIME_ZONE_OFFSET = 0x882A;
        public const int SELF_TIMER_MODE = 0x882B;
        public const int FLASH_ENERGY = 0x920B;
        public const int SPATIAL_FREQUENCY_RESPONSE = 0x920C;
        public const int NOISE = 0x920D;
        public const int IMAGE_NUMBER = 0x9211;
        public const int SECURITY_CLASSIFICATION = 0x9212;
        public const int IMAGE_HISTORY = 0x9213;
        public const int SUBJECT_LOCATION = 0x9214;
        public const int EXPOSURE_INDEX = 0x9215;
        public const int TIFF_EP_STANDARD_ID = 0x9216;
        public const int SUB_SEC_TIME = 0x9290;
        public const int SUB_SEC_TIME_ORIGINAL = 0x9291;
        public const int SUB_SEC_TIME_DIGITIZED = 0x9292;
        public const int FLASH_ENERGY_FOCAL_PLANE = 0xA20B;
        public const int SPATIAL_FREQUENCY_RESPONSE_FOCAL_PLANE = 0xA20C;
        public const int SUBJECT_LOCATION_FOCAL_PLANE = 0xA214;
        public const int EXPOSURE_INDEX_FOCAL_PLANE = 0xA215;
        public const int CFA_PATTERN_FOCAL_PLANE = 0xA302;

        // Common extension (not in your document but commonly used)
        public const int FOCAL_LENGTH_35MM = 0xA405;

        // Olympus MakerNote Tags (from appendix)
        public const int OLYMPUS_SPECIAL_MODE = 0x0200;
        public const int OLYMPUS_JPEG_QUALITY = 0x0201;
        public const int OLYMPUS_MACRO = 0x0202;
        public const int OLYMPUS_UNKNOWN_1 = 0x0203;
        public const int OLYMPUS_DIGITAL_ZOOM = 0x0204;
        public const int OLYMPUS_UNKNOWN_2 = 0x0205;
        public const int OLYMPUS_UNKNOWN_3 = 0x0206;
        public const int OLYMPUS_SOFTWARE_RELEASE = 0x0207;
        public const int OLYMPUS_PICTURE_INFO = 0x0208;
        public const int OLYMPUS_CAMERA_ID = 0x0209;
        public const int OLYMPUS_DATA_DUMP = 0x0F00;

        // GPS Tags
        public const int GPS_VERSION_ID = 0x0000;
        public const int GPS_LATITUDE_REF = 0x0001;
        public const int GPS_LATITUDE = 0x0002;
        public const int GPS_LONGITUDE_REF = 0x0003;
        public const int GPS_LONGITUDE = 0x0004;
        public const int GPS_ALTITUDE_REF = 0x0005;
        public const int GPS_ALTITUDE = 0x0006;
        public const int GPS_TIME_STAMP = 0x0007;
        public const int GPS_SATELLITES = 0x0008;
        public const int GPS_STATUS = 0x0009;
        public const int GPS_MEASURE_MODE = 0x000A;
        public const int GPS_DOP = 0x000B;
        public const int GPS_SPEED_REF = 0x000C;
        public const int GPS_SPEED = 0x000D;
        public const int GPS_TRACK_REF = 0x000E;
        public const int GPS_TRACK = 0x000F;
        public const int GPS_IMG_DIRECTION_REF = 0x0010;
        public const int GPS_IMG_DIRECTION = 0x0011;
        public const int GPS_MAP_DATUM = 0x0012;
        public const int GPS_DEST_LATITUDE_REF = 0x0013;
        public const int GPS_DEST_LATITUDE = 0x0014;
        public const int GPS_DEST_LONGITUDE_REF = 0x0015;
        public const int GPS_DEST_LONGITUDE = 0x0016;
        public const int GPS_DEST_BEARING_REF = 0x0017;
        public const int GPS_DEST_BEARING = 0x0018;
        public const int GPS_DEST_DISTANCE_REF = 0x0019;
        public const int GPS_DEST_DISTANCE = 0x001A;
        public const int GPS_PROCESSING_METHOD = 0x001B;
        public const int GPS_AREA_INFORMATION = 0x001C;
        public const int GPS_DATE_STAMP = 0x001D;
        public const int GPS_DIFFERENTIAL = 0x001E;

        // MISC, an attempt to fix dates
        public const int THUMBNAIL_DATE_TIME = 0x5033;
        public const int PREVIEW_DATE_TIME = 0xc71b;
    }
}
