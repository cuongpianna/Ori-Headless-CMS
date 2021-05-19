import {apiClient} from './api-client'


class MetadataService{
    async getFieldsMetadata() {
        try {
            const excludedTypes = ['fields-list', 'entities-list'];
            const res = await apiClient.get(`/system/metadata/fieldinfo`);
            return res.data.reduce((map, obj) => {
                if (!excludedTypes.includes(obj.type.typeName)) {
                    map[obj.type.typeName] = obj;
                }
                return map;
            }, {});
        } catch (e) {
            // snackbarService.showMessage({
            //     color: 'error',
            //     message: e,
            // });
        }
    }
}

export const metadataService = new MetadataService();
export default metadataService;